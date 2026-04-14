using System.ComponentModel.DataAnnotations;

namespace KoffiemachineServiceAPI.Models;

public class MaintenanceAction
{
    public Guid Id { get; set; }

    public Guid MachineId { get; set; }

    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string PerformedBy { get; set; } = string.Empty;

    public DateTime PerformedAt { get; set; } = DateTime.UtcNow;

    public Machine Machine { get; set; } = null!;
}
