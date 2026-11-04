using System.ComponentModel.DataAnnotations;

namespace DoomTracker.Models;

public class DebtLog
{
    public int Id { get; set; }
    public int DebtId { get; set; }
    public Debt Debt { get; set; } = null!;
    public DebtActionType ActionType { get; set; }
    public DebtStatus? OldStatus { get; set; }
    public DebtStatus NewStatus { get; set; }
    [MaxLength(255)]
    public string? LogMessage { get; set; }
    public DateTime CreatedAt { get; set; } =  DateTime.UtcNow;
}