using KoffiemachineServiceAPI.DTOs;
using KoffiemachineServiceAPI.Services;

namespace KoffiemachineServiceAPI.Tests;

public class MaintenanceServiceTests
{
    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenMachineNotFound()
    {
        using var context = TestDbContextFactory.Create();
        var service = new MaintenanceService(context);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.CreateAsync(Guid.NewGuid(), new CreateMaintenanceRequest
            {
                Description = "Test",
                PerformedBy = "Jan"
            }));
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateAction_WhenMachineExists()
    {
        using var context = TestDbContextFactory.Create();
        var machineService = new MachineService(context);
        var service = new MaintenanceService(context);

        var machine = await machineService.CreateAsync(new CreateMachineRequest
        {
            Name = "Machine",
            Location = "Test",
            Status = "Actief"
        });

        var result = await service.CreateAsync(machine.Id, new CreateMaintenanceRequest
        {
            Description = "Waterfilter vervangen",
            PerformedBy = "Technicus Jan"
        });

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(machine.Id, result.MachineId);
        Assert.Equal("Waterfilter vervangen", result.Description);
        Assert.Equal("Technicus Jan", result.PerformedBy);
    }

    [Fact]
    public async Task GetByMachineIdAsync_ShouldFilterByPerformedBy()
    {
        using var context = TestDbContextFactory.Create();
        var machineService = new MachineService(context);
        var service = new MaintenanceService(context);

        var machine = await machineService.CreateAsync(new CreateMachineRequest
        {
            Name = "Machine",
            Location = "Test",
            Status = "Actief"
        });

        await service.CreateAsync(machine.Id, new CreateMaintenanceRequest
        {
            Description = "Actie 1",
            PerformedBy = "Jan"
        });
        await service.CreateAsync(machine.Id, new CreateMaintenanceRequest
        {
            Description = "Actie 2",
            PerformedBy = "Piet"
        });
        await service.CreateAsync(machine.Id, new CreateMaintenanceRequest
        {
            Description = "Actie 3",
            PerformedBy = "Jan"
        });

        var result = await service.GetByMachineIdAsync(machine.Id, 1, 50, "Jan");

        Assert.Equal(2, result.TotalRecords);
        Assert.All(result.Data, a => Assert.Contains("Jan", a.PerformedBy));
    }

    [Fact]
    public async Task GetByMachineIdAsync_ShouldRespectPagination()
    {
        using var context = TestDbContextFactory.Create();
        var machineService = new MachineService(context);
        var service = new MaintenanceService(context);

        var machine = await machineService.CreateAsync(new CreateMachineRequest
        {
            Name = "Machine",
            Location = "Test",
            Status = "Actief"
        });

        for (var i = 0; i < 15; i++)
        {
            await service.CreateAsync(machine.Id, new CreateMaintenanceRequest
            {
                Description = $"Actie {i}",
                PerformedBy = "Jan"
            });
        }

        var result = await service.GetByMachineIdAsync(machine.Id, 2, 10, null);

        Assert.Equal(15, result.TotalRecords);
        Assert.Equal(2, result.CurrentPage);
        Assert.Equal(5, result.Data.Count);
    }
}
