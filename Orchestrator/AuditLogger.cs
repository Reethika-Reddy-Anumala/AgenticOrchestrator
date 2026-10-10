namespace Orchestrator;

public static class AuditLogger
{
    private static readonly string LogFile = "audit.log";

    public static void Log(string message)
    {
        var entry = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] {message}";
        Console.WriteLine(entry);
        File.AppendAllText(LogFile, entry + Environment.NewLine);
    }
}
