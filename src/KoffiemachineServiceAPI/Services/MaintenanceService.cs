using KoffiemachineServiceAPI.Data;
using KoffiemachineServiceAPI.DTOs;
using KoffiemachineServiceAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace KoffiemachineServiceAPI.Services;

public class MaintenanceService : IMaintenanceService
{
    private readonly AppDbContext _context;

    public MaintenanceService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<MaintenanceResponse> CreateAsync(Guid machineId, CreateMaintenanceRequest request)
    {
        // Verify the machine exists before logging maintenance
        var machineExists = await _context.Machines.AnyAsync(m => m.Id == machineId);
        if (!machineExists)
        {
            throw new KeyNotFoundException($"Machine with id '{machineId}' not found.");
        }

        var action = new MaintenanceAction
        {
            Id = Guid.NewGuid(),
            MachineId = machineId,
            Description = request.Description,
            PerformedBy = request.PerformedBy,
            PerformedAt = DateTime.UtcNow
        };

        _context.MaintenanceActions.Add(action);
        await _context.SaveChangesAsync();

        return MapToResponse(action);
    }

    public async Task<PagedResult<MaintenanceResponse>> GetByMachineIdAsync(
        Guid machineId, int page, int pageSize, string? performedBy)
    {
        // Verify the machine exists
        var machineExists = await _context.Machines.AnyAsync(m => m.Id == machineId);
        if (!machineExists)
        {
            throw new KeyNotFoundException($"Machine with id '{machineId}' not found.");
        }

        var query = _context.MaintenanceActions
            .AsNoTracking()
            .Where(ma => ma.MachineId == machineId);

        if (!string.IsNullOrWhiteSpace(performedBy))
        {
            query = query.Where(ma => ma.PerformedBy.Contains(performedBy));
        }

        var totalRecords = await query.CountAsync();

        var actions = await query
            .OrderByDescending(ma => ma.PerformedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(ma => MapToResponse(ma))
            .ToListAsync();

        return new PagedResult<MaintenanceResponse>
        {
            TotalRecords = totalRecords,
            PageSize = pageSize,
            CurrentPage = page,
            Data = actions
        };
    }

    private static MaintenanceResponse MapToResponse(MaintenanceAction action)
    {
        return new MaintenanceResponse
        {
            Id = action.Id,
            MachineId = action.MachineId,
            Description = action.Description,
            PerformedBy = action.PerformedBy,
            PerformedAt = action.PerformedAt
        };
    }
}
