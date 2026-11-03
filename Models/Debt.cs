using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace DoomTracker.Models;

public class Debt
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; }
    [Required, MaxLength(100)]
    public string Title { get; set; }
    public string? Description { get; set; }
    public DateTime Deadline { get; set; }
    public DebtStatus Status { get; set; } = DebtStatus.Pending;
    public string? ProofFilePath { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<DebtLog> Logs { get; set; } = new List<DebtLog>();
}