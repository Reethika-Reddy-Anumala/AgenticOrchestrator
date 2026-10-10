using System.Text.Json;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

namespace Orchestrator;

public class AgentLoop
{
    private readonly ChatClient _chatClient;
    private readonly HttpClient _toolClient;

    private const string Model      = "claude-haiku-4-5-20251001";
    private const int    MaxRetries = 3;

    private static string LlmBaseUrl    => Environment.GetEnvironmentVariable("LLM_BASE_URL")    ?? "https://your-llm-endpoint.example.com";
    private static string ToolServerUrl => Environment.GetEnvironmentVariable("TOOL_SERVER_URL") ?? "http://localhost:8000";

    private const string SystemPrompt =
        "You are a senior software engineer. You have access to tools to write files, " +
        "run builds, and scaffold projects. Use them to actually build what is asked. " +
        "Always call tools to produce real artifacts — do not just describe what you would do. " +
        "The sandbox directory is already created. Use relative paths for write_file.";

    // Tool definitions passed to Claude so it knows what tools exist
    private static readonly List<ChatTool> Tools =
    [
        ChatTool.CreateFunctionTool("write_file",
            "Write a source code file to disk",
            BinaryData.FromString("""
            {
              "type": "object",
              "properties": {
                "path":    { "type": "string", "description": "Relative file path e.g. Program.cs or Models/Url.cs" },
                "content": { "type": "string", "description": "Full file content to write" }
              },
              "required": ["path","content"]
            }
            """)),

        ChatTool.CreateFunctionTool("read_file",
            "Read a file from disk",
            BinaryData.FromString("""
            {
              "type": "object",
              "properties": {
                "path": { "type": "string", "description": "Relative file path to read" }
              },
              "required": ["path"]
            }
            """)),

        ChatTool.CreateFunctionTool("create_project",
            "Scaffold a new ASP.NET Core webapi project",
            BinaryData.FromString("""
            {
              "type": "object",
              "properties": {
                "name": { "type": "string", "description": "Project name" }
              },
              "required": ["name"]
            }
            """)),

        ChatTool.CreateFunctionTool("add_package",
            "Add a NuGet package to the project",
            BinaryData.FromString("""
            {
              "type": "object",
              "properties": {
                "package": { "type": "string", "description": "Package name e.g. Microsoft.EntityFrameworkCore.Sqlite" }
              },
              "required": ["package"]
            }
            """)),

        ChatTool.CreateFunctionTool("run_build",
            "Build the project and return compiler output",
            BinaryData.FromString("""{ "type": "object", "properties": {} }""")),

        ChatTool.CreateFunctionTool("run_tests",
            "Run tests and return results",
            BinaryData.FromString("""{ "type": "object", "properties": {} }""")),
    ];

    public AgentLoop(string apiKey)
    {
        var client = new OpenAIClient(
            new ApiKeyCredential(apiKey),
            new OpenAIClientOptions { Endpoint = new Uri(LlmBaseUrl) });
        _chatClient = client.GetChatClient(Model);
        _toolClient = new HttpClient { BaseAddress = new Uri(ToolServerUrl) };
    }

    public async Task<string> RunStageAsync(string stage, string context)
    {
        AuditLogger.Log($"Stage [{stage}] — STARTED");

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(SystemPrompt),
            new UserChatMessage(BuildPrompt(stage, context))
        };

        var attempt = 0;
        while (attempt < MaxRetries)
        {
            try
            {
                attempt++;
                var finalOutput = await RunReActLoop(messages, stage);
                AuditLogger.Log($"Stage [{stage}] — COMPLETED (attempt {attempt})");
                return finalOutput;
            }
            catch (Exception ex)
            {
                AuditLogger.Log($"Stage [{stage}] — ERROR attempt {attempt}: {ex.Message}");
                if (attempt >= MaxRetries)
                {
                    AuditLogger.Log($"Stage [{stage}] — FAILED after {MaxRetries} retries. Safe-stopping.");
                    throw;
                }
                await Task.Delay(2000);
            }
        }

        return string.Empty;
    }

    // ReAct loop: Claude thinks → calls tool → sees result → repeats until done
    private async Task<string> RunReActLoop(List<ChatMessage> messages, string stage)
    {
        while (true)
        {
            var options = new ChatCompletionOptions();
            foreach (var tool in Tools) options.Tools.Add(tool);

            var response = await _chatClient.CompleteChatAsync(messages, options);
            var choice   = response.Value;

            if (choice.FinishReason == ChatFinishReason.ToolCalls)
            {
                // Add Claude's response to history
                messages.Add(new AssistantChatMessage(choice));

                // Execute each tool call
                foreach (var toolCall in choice.ToolCalls)
                {
                    AuditLogger.Log($"  Tool call: {toolCall.FunctionName}");
                    var result = await CallToolServer(toolCall.FunctionName, toolCall.FunctionArguments);
                    AuditLogger.Log($"  Tool result: {result[..Math.Min(100, result.Length)]}...");

                    messages.Add(new ToolChatMessage(toolCall.Id, result));
                }
            }
            else if (choice.FinishReason == ChatFinishReason.Stop)
            {
                return choice.Content[0].Text ?? string.Empty;
            }
            else
            {
                return $"Stopped: {choice.FinishReason}";
            }
        }
    }

    private async Task<string> CallToolServer(string toolName, BinaryData args)
    {
        var payload = new
        {
            tool   = toolName,
            @params = JsonSerializer.Deserialize<JsonElement>(args.ToString())
        };

        var json    = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        var resp    = await _toolClient.PostAsync("/tool", content);
        return await resp.Content.ReadAsStringAsync();
    }

    private static string BuildPrompt(string stage, string context) => stage switch
    {
        "Requirements" =>
            $"Analyze this requirement and list clear engineering tasks with dependencies:\n\n{context}",

        "Architecture" =>
            $"Based on these requirements, design a simple architecture. List components and data flow:\n\n{context}",

        "Code" =>
            "Build a complete URL shortener ASP.NET Core Web API with these steps:\n" +
            "1. Call create_project to scaffold 'UrlShortenerApi'\n" +
            "2. Call add_package for Microsoft.EntityFrameworkCore.Sqlite\n" +
            "3. Call write_file to create Models/ShortenedUrl.cs\n" +
            "4. Call write_file to create Data/AppDbContext.cs\n" +
            "5. Call write_file to replace Program.cs with 3 endpoints: POST /shorten, GET /{code}, GET /analytics/{code}\n" +
            "6. Call run_build to verify it compiles\n" +
            "7. If build fails, fix the errors and build again\n\n" +
            $"Context:\n{context}",

        "Tests" =>
            "Write unit tests for the URL shortener. Call write_file to create test files, then call run_tests.\n\n" +
            $"Context:\n{context}",

        "Docs" =>
            $"Write a README.md for the URL shortener API. Call write_file to save it as README.md:\n\n{context}",

        "Deploy" =>
            $"Write a deployment checklist as DEPLOY.md. Call write_file to save it:\n\n{context}",

        _ => context
    };
}
