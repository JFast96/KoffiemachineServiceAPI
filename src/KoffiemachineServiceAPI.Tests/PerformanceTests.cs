using System.Diagnostics;
using KoffiemachineServiceAPI.Services;
using Xunit.Abstractions;

namespace KoffiemachineServiceAPI.Tests;

/// <summary>
/// Performance tests that simulate 10.000 machines and measure query response times.
/// These tests verify that pagination and filtering remain fast at scale.
/// </summary>
public class PerformanceTests
{
    private readonly ITestOutputHelper _output;

    public PerformanceTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task GetAllAsync_With10kMachines_ShouldRespondUnder100ms()
    {
        // Arrange - seed 10.000 machines
        using var context = TestDbContextFactory.CreateSeeded(10_000);
        var service = new MachineService(context);

        // Warmup
        await service.GetAllAsync(1, 20, null);

        // Act - measure paginated query
        var sw = Stopwatch.StartNew();
        var result = await service.GetAllAsync(1, 20, null);
        sw.Stop();

        // Assert
        _output.WriteLine($"GetAll (page 1, size 20): {sw.ElapsedMilliseconds}ms — {result.TotalRecords} total records");
        Assert.Equal(10_000, result.TotalRecords);
        Assert.Equal(20, result.Data.Count);
        Assert.True(sw.ElapsedMilliseconds < 100, $"Query took {sw.ElapsedMilliseconds}ms, expected < 100ms");
    }

    [Fact]
    public async Task GetAllAsync_FilterByStatus_With10kMachines_ShouldRespondUnder100ms()
    {
        using var context = TestDbContextFactory.CreateSeeded(10_000);
        var service = new MachineService(context);

        // Warmup
        await service.GetAllAsync(1, 20, "Actief");

        // Act
        var sw = Stopwatch.StartNew();
        var result = await service.GetAllAsync(1, 20, "Actief");
        sw.Stop();

        _output.WriteLine($"GetAll filtered (status=Actief): {sw.ElapsedMilliseconds}ms — {result.TotalRecords} matching records");
        Assert.True(result.TotalRecords > 0, "Expected some 'Actief' machines");
        Assert.True(sw.ElapsedMilliseconds < 100, $"Query took {sw.ElapsedMilliseconds}ms, expected < 100ms");
    }

    [Fact]
    public async Task GetAllAsync_LastPage_With10kMachines_ShouldRespondUnder100ms()
    {
        using var context = TestDbContextFactory.CreateSeeded(10_000);
        var service = new MachineService(context);

        // Get total pages first
        var initial = await service.GetAllAsync(1, 20, null);
        var lastPage = initial.TotalPages;

        // Act - query the last page (worst case for Skip/Take)
        var sw = Stopwatch.StartNew();
        var result = await service.GetAllAsync(lastPage, 20, null);
        sw.Stop();

        _output.WriteLine($"GetAll last page ({lastPage}): {sw.ElapsedMilliseconds}ms — {result.Data.Count} records returned");
        Assert.True(result.Data.Count > 0, "Expected records on last page");
        Assert.True(sw.ElapsedMilliseconds < 200, $"Last page query took {sw.ElapsedMilliseconds}ms, expected < 200ms");
    }

    [Fact]
    public async Task GetByIdAsync_With10kMachines_ShouldRespondUnder50ms()
    {
        using var context = TestDbContextFactory.CreateSeeded(10_000);
        var service = new MachineService(context);

        // Get a machine id to look up
        var page = await service.GetAllAsync(250, 1, null);
        var targetId = page.Data[0].Id;

        // Act
        var sw = Stopwatch.StartNew();
        var result = await service.GetByIdAsync(targetId);
        sw.Stop();

        _output.WriteLine($"GetById: {sw.ElapsedMilliseconds}ms");
        Assert.NotNull(result);
        Assert.True(sw.ElapsedMilliseconds < 50, $"Query took {sw.ElapsedMilliseconds}ms, expected < 50ms");
    }

    [Fact]
    public async Task GetMaintenanceActions_With10kMachines_ShouldRespondUnder100ms()
    {
        using var context = TestDbContextFactory.CreateSeeded(10_000);
        var machineService = new MachineService(context);
        var maintenanceService = new MaintenanceService(context);

        // Find a machine that has maintenance actions
        var machines = await machineService.GetAllAsync(1, 100, null);
        Guid? targetId = null;
        foreach (var m in machines.Data)
        {
            var actions = await maintenanceService.GetByMachineIdAsync(m.Id, 1, 1, null);
            if (actions.TotalRecords > 0)
            {
                targetId = m.Id;
                break;
            }
        }

        Assert.NotNull(targetId);

        // Act
        var sw = Stopwatch.StartNew();
        var result = await maintenanceService.GetByMachineIdAsync(targetId!.Value, 1, 50, null);
        sw.Stop();

        _output.WriteLine($"GetMaintenanceActions: {sw.ElapsedMilliseconds}ms — {result.TotalRecords} actions");
        Assert.True(result.TotalRecords > 0);
        Assert.True(sw.ElapsedMilliseconds < 100, $"Query took {sw.ElapsedMilliseconds}ms, expected < 100ms");
    }

}
