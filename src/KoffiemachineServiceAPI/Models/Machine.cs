using System.ComponentModel.DataAnnotations;

namespace KoffiemachineServiceAPI.Models;

public class Machine
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Location { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "Actief";

    /// <summary>
    /// Used for optimistic concurrency control.
    /// Manually managed as a GUID string because EF Core InMemory
    /// does not support database-generated row versions.
    /// </summary>
    [ConcurrencyCheck]
    public string RowVersion { get; set; } = Guid.NewGuid().ToString();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public ICollection<MaintenanceAction> MaintenanceActions { get; set; } = new List<MaintenanceAction>();
}
