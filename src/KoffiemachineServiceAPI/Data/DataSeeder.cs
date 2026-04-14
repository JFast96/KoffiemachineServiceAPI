using KoffiemachineServiceAPI.Models;

namespace KoffiemachineServiceAPI.Data;

public static class DataSeeder
{
    private static readonly string[] Locations =
    [
        "Kantine", "Receptie", "Vergaderzaal A", "Vergaderzaal B", "Directiekamer",
        "IT-afdeling", "HR-afdeling", "Marketing", "Productiehal", "Magazijn",
        "Lobby", "Derde verdieping", "Pantry 1", "Pantry 2", "Cafetaria"
    ];

    private static readonly string[] Statuses = ["Actief", "Inactief", "Defect", "In onderhoud"];

    private static readonly string[] Technicians =
    [
        "Technicus Jan", "Technicus Piet", "Technicus Klaas",
        "Technicus Maria", "Technicus Sophie"
    ];

    private static readonly string[] MaintenanceDescriptions =
    [
        "Verwarmingselement vervangen", "Waterfilter vervangen", "Ontkalking uitgevoerd",
        "Maalwerk gereinigd", "Lekbak vervangen", "Software update geinstalleerd",
        "Melkslang vervangen", "Algemene inspectie", "Bonenreservoir gereinigd",
        "Waterreservoir gereinigd"
    ];

    public static void Seed(AppDbContext context, int machineCount = 10_000)
    {
        if (context.Machines.Any())
        {
            return; // Already seeded
        }

        var random = new Random(42); // Fixed seed for reproducibility
        var machines = new List<Machine>(machineCount);

        for (var i = 1; i <= machineCount; i++)
        {
            var machine = new Machine
            {
                Id = Guid.NewGuid(),
                Name = $"Koffiemachine {i:D5}",
                Location = Locations[random.Next(Locations.Length)],
                Status = Statuses[random.Next(Statuses.Length)],
                RowVersion = Guid.NewGuid().ToString(),
                CreatedAt = DateTime.UtcNow.AddDays(-random.Next(1, 365))
            };

            // Add 0-3 maintenance actions per machine
            var maintenanceCount = random.Next(0, 4);
            for (var j = 0; j < maintenanceCount; j++)
            {
                machine.MaintenanceActions.Add(new MaintenanceAction
                {
                    Id = Guid.NewGuid(),
                    MachineId = machine.Id,
                    Description = MaintenanceDescriptions[random.Next(MaintenanceDescriptions.Length)],
                    PerformedBy = Technicians[random.Next(Technicians.Length)],
                    PerformedAt = DateTime.UtcNow.AddDays(-random.Next(1, 180))
                });
            }

            machines.Add(machine);
        }

        context.Machines.AddRange(machines);
        context.SaveChanges();
    }
}
