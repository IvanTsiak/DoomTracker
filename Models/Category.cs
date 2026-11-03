using System.ComponentModel.DataAnnotations;

namespace DoomTracker.Models;

public class Category
{
    public int Id { get; set; }
    [Required, MaxLength(50)]
    public string Name { get; set; }
    public bool IsSystem { get; set; }
    public ICollection<Debt> Debts { get; set; } = new List<Debt>();
}