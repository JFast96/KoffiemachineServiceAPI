using KoffiemachineServiceAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace KoffiemachineServiceAPI.Tests;

/// <summary>
/// Creates a fresh in-memory database for each test to ensure isolation.
/// </summary>
public static class TestDbContextFactory
{
    public static AppDbContext Create(string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString())
            .Options;

        var context = new AppDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    public static AppDbContext CreateSeeded(int machineCount = 10_000, string? dbName = null)
    {
        var context = Create(dbName);
        DataSeeder.Seed(context, machineCount);
        return context;
    }
}
