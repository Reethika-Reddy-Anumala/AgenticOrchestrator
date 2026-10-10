namespace Orchestrator;

public static class HumanCheckpoint
{
    public static bool RequestApproval(string stage, string summary)
    {
        Console.WriteLine();
        Console.WriteLine("===========================================");
        Console.WriteLine($"  HUMAN CHECKPOINT — Stage: {stage}");
        Console.WriteLine("===========================================");
        Console.WriteLine(summary);
        Console.WriteLine();
        Console.Write("Approve and continue? (y/n): ");
        var input = Console.ReadLine();
        var approved = input?.Trim().ToLower() == "y";
        AuditLogger.Log($"Checkpoint [{stage}] — {(approved ? "APPROVED" : "REJECTED")} by human");
        return approved;
    }
}
