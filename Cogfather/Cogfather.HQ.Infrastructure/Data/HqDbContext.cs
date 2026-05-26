using System.Text.Json;
using Cogfather.HQ.Domain.Entities;
using Cogfather.HQ.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Cogfather.HQ.Infrastructure.Data;

public class HqDbContext : DbContext
{
    public HqDbContext(DbContextOptions<HqDbContext> options) : base(options)
    {
    }

    public DbSet<HqInventory> Inventories { get; set; } = null!;
    public DbSet<NodeRegistration> Nodes { get; set; } = null!;
    public DbSet<ProductionOrder> ProductionOrders { get; set; } = null!;
    public DbSet<ProductionReport> ProductionReports { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<HqInventory>(b =>
        {
            b.HasKey(i => i.Id);

            b.Property(i => i.Items)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<Dictionary<string, double>>(v, (JsonSerializerOptions?)null) ??
                         new Dictionary<string, double>())
                .Metadata.SetValueComparer(new ValueComparer<IReadOnlyDictionary<string, double>>(
                    (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.Key.GetHashCode(), v.Value.GetHashCode())),
                    c => c.ToDictionary(k => k.Key, v => v.Value)));
        });

        modelBuilder.Entity<NodeRegistration>(b =>
        {
            b.HasKey(n => n.NodeId);
            b.Property(n => n.NodeId).IsRequired().HasMaxLength(100);
            b.Property(n => n.Address).IsRequired().HasMaxLength(500);
            b.Property(n => n.Status).HasConversion(
                v => v.ToString(),
                v => Enum.Parse<NodeStatus>(v));
            b.Property(n => n.FaultMode).HasConversion(
                v => v.ToString(),
                v => Enum.Parse<FaultMode>(v));
        });

        modelBuilder.Entity<ProductionOrder>(b =>
        {
            b.HasKey(o => o.Id);
            b.Property(o => o.RecipeId).IsRequired().HasMaxLength(100);
            b.Property(o => o.Status).HasConversion(
                v => v.ToString(),
                v => Enum.Parse<ProductionOrderStatus>(v));
        });

        modelBuilder.Entity<ProductionReport>(b =>
        {
            b.HasKey(r => r.Id);
            b.Property(r => r.NodeId).IsRequired().HasMaxLength(100);
            b.Property(r => r.RecipeId).IsRequired().HasMaxLength(100);
        });
    }
}