using KoffiemachineServiceAPI.DTOs;

namespace KoffiemachineServiceAPI.Services;

public interface IMaintenanceService
{
    Task<MaintenanceResponse> CreateAsync(Guid machineId, CreateMaintenanceRequest request);
    Task<PagedResult<MaintenanceResponse>> GetByMachineIdAsync(Guid machineId, int page, int pageSize, string? performedBy);
}
