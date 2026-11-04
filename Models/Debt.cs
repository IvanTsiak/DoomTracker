using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc;

namespace DoomTracker.Models;

public class Debt
{
    public int Id { get; set; }
    public int UserId { get; set; }
    [ValidateNever]
    public User User { get; set; } = null!;
    public int CategoryId { get; set; }
    [ValidateNever]
    public Category Category { get; set; } = null!;
    [Required, MaxLength(100)]
    public string Title { get; set; }
    public string? Description { get; set; }
    public DateTime Deadline { get; set; }
    public DebtStatus Status { get; set; } = DebtStatus.Pending;
    public string? ProofFilePath { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ValidateNever]
    public ICollection<DebtLog> Logs { get; set; } = new List<DebtLog>();
}