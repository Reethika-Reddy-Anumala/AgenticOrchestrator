using Orchestrator;

Console.WriteLine("===========================================");
Console.WriteLine("   URL Shortener — Agentic Orchestrator   ");
Console.WriteLine("===========================================");
Console.WriteLine();

// Get API key
var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? string.Empty;
if (string.IsNullOrEmpty(apiKey))
{
    Console.Write("Enter your Anthropic API key: ");
    apiKey = Console.ReadLine() ?? string.Empty;
}

// The requirement fed into the agent
var requirement = "Build a URL shortener service with: " +
                  "1) POST /shorten to shorten a URL, " +
                  "2) GET /{code} to redirect, " +
                  "3) GET /analytics/{code} to get click stats. " +
                  "Use ASP.NET Core, SQLite, and in-memory caching.";

var agent = new AgentLoop(apiKey);
var stageManager = new StageManager();
var context = requirement;

AuditLogger.Log("Orchestrator started");
AuditLogger.Log($"Requirement: {requirement}");

while (!stageManager.AllDone())
{
    var readyStages = stageManager.GetReadyStages().ToList();

    foreach (var stage in readyStages)
    {
        // Human must approve before Deploy
        if (stage == "Deploy")
        {
            var approved = HumanCheckpoint.RequestApproval(stage,
                "The agent is about to generate the deployment checklist. All tests and docs are complete.");
            if (!approved)
            {
                AuditLogger.Log("Deploy stage rejected by human. Stopping.");
                Console.WriteLine("Deploy rejected. Orchestration stopped.");
                return;
            }
        }

        Console.WriteLine();
        Console.WriteLine($">>> Running stage: {stage}");

        var output = await agent.RunStageAsync(stage, context);

        // Save output to a file per stage
        var fileName = $"output_{stage.ToLower()}.md";
        await File.WriteAllTextAsync(fileName, $"# {stage}\n\n{output}");
        Console.WriteLine($"    Output saved to {fileName}");

        // Pass this stage's output as context to the next stage
        context = output;

        stageManager.MarkCompleted(stage);
    }
}

AuditLogger.Log("All stages completed successfully.");
Console.WriteLine();
Console.WriteLine("===========================================");
Console.WriteLine("   Orchestration Complete!");
Console.WriteLine("   Check output_*.md files for results.");
Console.WriteLine("   Check audit.log for full decision trail.");
Console.WriteLine("===========================================");
