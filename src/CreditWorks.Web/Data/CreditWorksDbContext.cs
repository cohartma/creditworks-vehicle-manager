using CreditWorks.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace CreditWorks.Web.Data;

public class CreditWorksDbContext : DbContext
{
    public CreditWorksDbContext(
            DbContextOptions<CreditWorksDbContext> options)
        : base(options)
    {
    }

    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Manufacturer> Manufacturers => Set<Manufacturer>();
    public DbSet<VehicleCategory> VehicleCategories => Set<VehicleCategory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Manufacturer>(entity =>
            {
                entity.Property(manufacturer => manufacturer.Name)
                .HasMaxLength(100)
                .IsRequired();

                entity.HasIndex(manufacturer => manufacturer.Name)
                .IsUnique();
            });

        modelBuilder.Entity<Vehicle>(entity =>
            {
                entity.Property(vehicle => vehicle.OwnerName)
                .HasMaxLength(200)
                .IsRequired();

                entity.Property(vehicle => vehicle.WeightKg)
                .HasPrecision(9, 2);

                entity.HasOne(vehicle => vehicle.Manufacturer)
                .WithMany(manufacturer => manufacturer.Vehicles)
                .HasForeignKey(vehicle => vehicle.ManufacturerId)
                .OnDelete(DeleteBehavior.Restrict);
            });

        modelBuilder.Entity<VehicleCategory>(entity =>
            {
                entity.Property(category => category.Name)
                .HasMaxLength(100)
                .IsRequired();

                entity.Property(category => category.IconKey)
                .HasMaxLength(50)
                .IsRequired();

                entity.Property(category => category.MinimumWeightKg)
                .HasPrecision(9, 2);

                entity.Property(category => category.MaximumWeightKg)
                .HasPrecision(9, 2);

            });
    }
}
