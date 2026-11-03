namespace DoomTracker.Models;

public enum DebtStatus
{
    Pending,
    InProgress,
    Completed,
    Failed
}

public enum DebtActionType
{
    Created,
    ProofUploaded,
    StatusChanged,
    DeadlineMissed
}