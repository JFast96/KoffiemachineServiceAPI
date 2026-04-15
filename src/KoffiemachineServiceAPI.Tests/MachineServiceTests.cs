using KoffiemachineServiceAPI.DTOs;
using KoffiemachineServiceAPI.Services;
using Microsoft.EntityFrameworkCore;

namespace KoffiemachineServiceAPI.Tests;

public class MachineServiceTests
{
    [Fact]
    public async Task CreateAsync_ShouldCreateMachine()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var service = new MachineService(context);
        var request = new CreateMachineRequest
        {
            Name = "Test Machine",
            Location = "Kantine",
            Status = "Actief"
        };

        // Act
        var result = await service.CreateAsync(request);

        // Assert
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Test Machine", result.Name);
        Assert.Equal("Kantine", result.Location);
        Assert.Equal("Actief", result.Status);
        Assert.False(string.IsNullOrEmpty(result.RowVersion));
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        using var context = TestDbContextFactory.Create();
        var service = new MachineService(context);

        var result = await service.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnMachine_WhenExists()
    {
        using var context = TestDbContextFactory.Create();
        var service = new MachineService(context);
        var created = await service.CreateAsync(new CreateMachineRequest
        {
            Name = "Machine X",
            Location = "Lobby",
            Status = "Actief"
        });

        var result = await service.GetByIdAsync(created.Id);

        Assert.NotNull(result);
        Assert.Equal("Machine X", result!.Name);
    }

    [Fact]
    public async Task GetAllAsync_ShouldRespectPagination()
    {
        using var context = TestDbContextFactory.Create();
        var service = new MachineService(context);

        // Create 25 machines
        for (var i = 0; i < 25; i++)
        {
            await service.CreateAsync(new CreateMachineRequest
            {
                Name = $"Machine {i:D3}",
                Location = "Test",
                Status = "Actief"
            });
        }

        // Act - request page 2 with pageSize 10
        var result = await service.GetAllAsync(2, 10, null);

        // Assert
        Assert.Equal(25, result.TotalRecords);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(2, result.CurrentPage);
        Assert.Equal(3, result.TotalPages);
        Assert.Equal(10, result.Data.Count);
    }

    [Fact]
    public async Task GetAllAsync_ShouldFilterByStatus()
    {
        using var context = TestDbContextFactory.Create();
        var service = new MachineService(context);

        await service.CreateAsync(new CreateMachineRequest { Name = "A", Location = "X", Status = "Actief" });
        await service.CreateAsync(new CreateMachineRequest { Name = "B", Location = "X", Status = "Defect" });
        await service.CreateAsync(new CreateMachineRequest { Name = "C", Location = "X", Status = "Actief" });

        var result = await service.GetAllAsync(1, 20, "Defect");

        Assert.Equal(1, result.TotalRecords);
        Assert.Equal("B", result.Data[0].Name);
    }

    [Fact]
    public async Task UpdateStatusAsync_ShouldThrow_WhenNotFound()
    {
        using var context = TestDbContextFactory.Create();
        var service = new MachineService(context);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.UpdateStatusAsync(Guid.NewGuid(), new UpdateStatusRequest
            {
                Status = "Defect",
                RowVersion = "fake"
            }));
    }

    [Fact]
    public async Task UpdateStatusAsync_ShouldUpdate_WhenRowVersionMatches()
    {
        using var context = TestDbContextFactory.Create();
        var service = new MachineService(context);
        var created = await service.CreateAsync(new CreateMachineRequest
        {
            Name = "Machine",
            Location = "Test",
            Status = "Actief"
        });

        var updated = await service.UpdateStatusAsync(created.Id, new UpdateStatusRequest
        {
            Status = "Defect",
            RowVersion = created.RowVersion
        });

        Assert.Equal("Defect", updated.Status);
        Assert.NotEqual(created.RowVersion, updated.RowVersion); // New version generated
    }

    [Fact]
    public async Task UpdateStatusAsync_ShouldThrowConcurrency_WhenRowVersionMismatch()
    {
        using var context = TestDbContextFactory.Create();
        var service = new MachineService(context);
        var created = await service.CreateAsync(new CreateMachineRequest
        {
            Name = "Machine",
            Location = "Test",
            Status = "Actief"
        });

        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() =>
            service.UpdateStatusAsync(created.Id, new UpdateStatusRequest
            {
                Status = "Defect",
                RowVersion = "wrong-version"
            }));
    }

    [Fact]
    public async Task UpdateStatusAsync_ShouldDetectConflict_WhenTwoUsersUpdateSameMachine()
    {
        // Simulates two users who both read the same machine,
        // then both try to update it. The second update should fail.

        using var context = TestDbContextFactory.Create();
        var service = new MachineService(context);

        // Both users see the machine in its original state
        var machine = await service.CreateAsync(new CreateMachineRequest
        {
            Name = "Shared Machine",
            Location = "Kantine",
            Status = "Actief"
        });

        var rowVersionSeenByUserA = machine.RowVersion;
        var rowVersionSeenByUserB = machine.RowVersion;

        // User A updates successfully — RowVersion changes in the database
        var updatedByA = await service.UpdateStatusAsync(machine.Id, new UpdateStatusRequest
        {
            Status = "In onderhoud",
            RowVersion = rowVersionSeenByUserA
        });

        // Verify User A's update went through
        Assert.Equal("In onderhoud", updatedByA.Status);
        Assert.NotEqual(rowVersionSeenByUserA, updatedByA.RowVersion);

        // User B tries to update with the OLD RowVersion — should fail
        var exception = await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() =>
            service.UpdateStatusAsync(machine.Id, new UpdateStatusRequest
            {
                Status = "Defect",
                RowVersion = rowVersionSeenByUserB  // stale!
            }));

        // Verify the database still has User A's update, not User B's
        var currentState = await service.GetByIdAsync(machine.Id);
        Assert.NotNull(currentState);
        Assert.Equal("In onderhoud", currentState!.Status);  // A's update persisted
        Assert.Equal(updatedByA.RowVersion, currentState.RowVersion);  // version matches A
    }
}
