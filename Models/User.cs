using System.ComponentModel.DataAnnotations;

namespace DoomTracker.Models;

public class User
{
    public int Id { get; set; }
    [Required, MaxLength(50)]
    public string Username { get; set; }
    [Required, EmailAddress, MaxLength(100)]
    public string Email { get; set; }
    [Required]
    public string PasswordHash { get; set; }
    public string? AvatarUrl { get; set; }
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    public ICollection<Debt> Debts { get; set;  } = new List<Debt>();
}