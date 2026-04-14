using KoffiemachineServiceAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace KoffiemachineServiceAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Machine> Machines => Set<Machine>();
    public DbSet<MaintenanceAction> MaintenanceActions => Set<MaintenanceAction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Machine>(entity =>
        {
            entity.HasKey(m => m.Id);

            entity.Property(m => m.Name).IsRequired().HasMaxLength(100);
            entity.Property(m => m.Location).IsRequired().HasMaxLength(200);
            entity.Property(m => m.Status).IsRequired().HasMaxLength(50);
            entity.Property(m => m.RowVersion).IsConcurrencyToken();

            entity.HasIndex(m => m.Status);

            entity.HasMany(m => m.MaintenanceActions)
                  .WithOne(ma => ma.Machine)
                  .HasForeignKey(ma => ma.MachineId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MaintenanceAction>(entity =>
        {
            entity.HasKey(ma => ma.Id);

            entity.Property(ma => ma.Description).IsRequired().HasMaxLength(500);
            entity.Property(ma => ma.PerformedBy).IsRequired().HasMaxLength(100);

            entity.HasIndex(ma => ma.MachineId);
            entity.HasIndex(ma => ma.PerformedBy);
        });
    }
}
