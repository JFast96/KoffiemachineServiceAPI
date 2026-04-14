using System.ComponentModel.DataAnnotations;

namespace KoffiemachineServiceAPI.DTOs;

public record CreateMachineRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; init; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Location { get; init; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Status { get; init; } = "Actief";
}

public record UpdateStatusRequest
{
    [Required]
    [MaxLength(50)]
    public string Status { get; init; } = string.Empty;

    [Required]
    public string RowVersion { get; init; } = string.Empty;
}

public record MachineResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Location { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string RowVersion { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
