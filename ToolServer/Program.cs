using System.Diagnostics;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var sandboxRoot = Path.GetFullPath(
    Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Generated"));
Directory.CreateDirectory(sandboxRoot);

app.MapGet("/tools", () => new[]
{
    new { name = "write_file",     description = "Write a file inside the generated project sandbox" },
    new { name = "read_file",      description = "Read a file inside the generated project sandbox" },
    new { name = "run_build",      description = "Run dotnet build on the generated project" },
    new { name = "run_tests",      description = "Run dotnet test on the generated project" },
    new { name = "create_project", description = "Scaffold a new dotnet webapi project inside the sandbox" },
    new { name = "add_package",    description = "Add a whitelisted NuGet package to the generated project" },
});

app.MapPost("/tool", async (HttpContext ctx) =>
{
    var req = await JsonSerializer.DeserializeAsync<ToolRequest>(ctx.Request.Body,
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    if (req is null) return Results.BadRequest("Invalid request");

    object result = req.Tool switch
    {
        "write_file"     => WriteFile(req.Params, sandboxRoot),
        "read_file"      => ReadFile(req.Params, sandboxRoot),
        "run_build"      => RunDotnet("build", sandboxRoot),
        "run_tests"      => RunDotnet("test", sandboxRoot),
        "create_project" => CreateProject(req.Params, sandboxRoot),
        "add_package"    => AddPackage(req.Params, sandboxRoot),
        _                => new { success = false, output = $"Unknown tool: {req.Tool}" }
    };

    return Results.Ok(new { result });
});

app.Run("http://localhost:8000");

static string SafePath(string rel, string sandbox)
{
    var full = Path.GetFullPath(Path.Combine(sandbox, rel));
    if (!full.StartsWith(sandbox, StringComparison.OrdinalIgnoreCase))
        throw new UnauthorizedAccessException("Path traversal not allowed.");
    return full;
}

static object WriteFile(JsonElement? p, string sandbox)
{
    var rel     = p?.GetProperty("path").GetString() ?? "";
    var content = p?.GetProperty("content").GetString() ?? "";
    var full    = SafePath(rel, sandbox);
    Directory.CreateDirectory(Path.GetDirectoryName(full)!);
    File.WriteAllText(full, content);
    return new { success = true, output = $"Written: {rel}" };
}

static object ReadFile(JsonElement? p, string sandbox)
{
    var rel  = p?.GetProperty("path").GetString() ?? "";
    var full = SafePath(rel, sandbox);
    if (!File.Exists(full)) return new { success = false, output = $"Not found: {rel}" };
    return new { success = true, output = File.ReadAllText(full) };
}

static object RunDotnet(string subcommand, string workingDir)
{
    var psi = new ProcessStartInfo("dotnet", subcommand)
    {
        WorkingDirectory       = workingDir,
        RedirectStandardOutput = true,
        RedirectStandardError  = true,
        UseShellExecute        = false,
    };
    using var proc = Process.Start(psi)!;
    var output = proc.StandardOutput.ReadToEnd() + proc.StandardError.ReadToEnd();
    proc.WaitForExit();
    return new { success = proc.ExitCode == 0, output };
}

static object CreateProject(JsonElement? p, string sandbox)
{
    var name = p?.GetProperty("name").GetString() ?? "GeneratedApi";
    var psi  = new ProcessStartInfo("dotnet", $"new webapi -n {name} --no-openapi -o \"{sandbox}\"")
    {
        RedirectStandardOutput = true,
        RedirectStandardError  = true,
        UseShellExecute        = false,
    };
    using var proc = Process.Start(psi)!;
    var output = proc.StandardOutput.ReadToEnd() + proc.StandardError.ReadToEnd();
    proc.WaitForExit();
    return new { success = proc.ExitCode == 0, output };
}

static object AddPackage(JsonElement? p, string sandbox)
{
    var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "Microsoft.EntityFrameworkCore.Sqlite",
        "Microsoft.EntityFrameworkCore.Design",
        "Microsoft.EntityFrameworkCore.Tools",
    };
    var package = p?.GetProperty("package").GetString() ?? "";
    if (!allowed.Contains(package))
        return new { success = false, output = $"Package '{package}' is not in the allowed list." };

    var psi = new ProcessStartInfo("dotnet", $"add package {package}")
    {
        WorkingDirectory       = sandbox,
        RedirectStandardOutput = true,
        RedirectStandardError  = true,
        UseShellExecute        = false,
    };
    using var proc = Process.Start(psi)!;
    var output = proc.StandardOutput.ReadToEnd() + proc.StandardError.ReadToEnd();
    proc.WaitForExit();
    return new { success = proc.ExitCode == 0, output };
}

record ToolRequest(string Tool, JsonElement? Params);
