namespace Orchestrator;

public class StageManager
{
    // Dependency graph — each stage lists what must complete before it can start
    private readonly Dictionary<string, string[]> _dependencies = new()
    {
        { "Requirements",  [] },
        { "Architecture",  ["Requirements"] },
        { "Code",          ["Architecture"] },
        { "Tests",         ["Code"] },
        { "Docs",          ["Code"] },           // runs in parallel with Tests
        { "Deploy",        ["Tests", "Docs"] }   // needs both to finish first
    };

    private readonly HashSet<string> _completed = [];

    public IEnumerable<string> GetReadyStages()
    {
        return _dependencies
            .Where(kvp => !_completed.Contains(kvp.Key) && kvp.Value.All(_completed.Contains))
            .Select(kvp => kvp.Key);
    }

    public void MarkCompleted(string stage)
    {
        _completed.Add(stage);
        AuditLogger.Log($"Stage [{stage}] — COMPLETED");
    }

    public bool AllDone() => _completed.Count == _dependencies.Count;
}
