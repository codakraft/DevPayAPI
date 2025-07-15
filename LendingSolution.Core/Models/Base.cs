using System.ComponentModel.DataAnnotations;

namespace LendingSolution.Core.Models;

public class Base
{
    [Key]
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Base()
    {
        Id = Guid.NewGuid();
    }
}

public class ExtendedBase: Base
{
    [MaxLength(450)]
    public string? CreatedBy { get; set; }

    [MaxLength(450)]
    public string? UpdatedBy { get; set; }
}
