using System.ComponentModel.DataAnnotations;

namespace KoffiemachineServiceAPI.DTOs;

public record CreateMaintenanceRequest
{
    [Required]
    [MaxLength(500)]
    public string Description { get; init; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string PerformedBy { get; init; } = string.Empty;
}

public record MaintenanceResponse
{
    public Guid Id { get; init; }
    public Guid MachineId { get; init; }
    public string Description { get; init; } = string.Empty;
    public string PerformedBy { get; init; } = string.Empty;
    public DateTime PerformedAt { get; init; }
}
