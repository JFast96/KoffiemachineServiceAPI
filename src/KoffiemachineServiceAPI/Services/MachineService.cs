using KoffiemachineServiceAPI.Data;
using KoffiemachineServiceAPI.DTOs;
using KoffiemachineServiceAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace KoffiemachineServiceAPI.Services;

public class MachineService : IMachineService
{
    private readonly AppDbContext _context;

    public MachineService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<MachineResponse> CreateAsync(CreateMachineRequest request)
    {
        var machine = new Machine
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Location = request.Location,
            Status = request.Status,
            RowVersion = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow
        };

        _context.Machines.Add(machine);
        await _context.SaveChangesAsync();

        return MapToResponse(machine);
    }

    public async Task<PagedResult<MachineResponse>> GetAllAsync(int page, int pageSize, string? status)
    {
        var query = _context.Machines.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(m => m.Status == status);
        }

        var totalRecords = await query.CountAsync();

        var machines = (await query
            .OrderBy(m => m.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync())
            .Select(MapToResponse)
            .ToList();

        return new PagedResult<MachineResponse>
        {
            TotalRecords = totalRecords,
            PageSize = pageSize,
            CurrentPage = page,
            Data = machines
        };
    }

    public async Task<MachineResponse?> GetByIdAsync(Guid id)
    {
        var machine = await _context.Machines
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id);

        return machine is null ? null : MapToResponse(machine);
    }

    public async Task<MachineResponse> UpdateStatusAsync(Guid id, UpdateStatusRequest request)
    {
        var machine = await _context.Machines.FindAsync(id);

        if (machine is null)
        {
            throw new KeyNotFoundException($"Machine with id '{id}' not found.");
        }

        // Optimistic concurrency check:
        // Compare the RowVersion the client sent with the current value in the database.
        // If they don't match, another client has modified this record in the meantime.
        if (machine.RowVersion != request.RowVersion)
        {
            throw new DbUpdateConcurrencyException(
                "The machine has been modified by another user. Please reload and try again.");
        }

        machine.Status = request.Status;
        machine.RowVersion = Guid.NewGuid().ToString();
        machine.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToResponse(machine);
    }

    private static MachineResponse MapToResponse(Machine machine)
    {
        return new MachineResponse
        {
            Id = machine.Id,
            Name = machine.Name,
            Location = machine.Location,
            Status = machine.Status,
            RowVersion = machine.RowVersion,
            CreatedAt = machine.CreatedAt,
            UpdatedAt = machine.UpdatedAt
        };
    }
}
