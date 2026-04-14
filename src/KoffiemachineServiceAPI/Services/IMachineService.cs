using KoffiemachineServiceAPI.DTOs;

namespace KoffiemachineServiceAPI.Services;

public interface IMachineService
{
    Task<MachineResponse> CreateAsync(CreateMachineRequest request);
    Task<PagedResult<MachineResponse>> GetAllAsync(int page, int pageSize, string? status);
    Task<MachineResponse?> GetByIdAsync(Guid id);
    Task<MachineResponse> UpdateStatusAsync(Guid id, UpdateStatusRequest request);
}
