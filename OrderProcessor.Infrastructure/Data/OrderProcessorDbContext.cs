using Microsoft.EntityFrameworkCore;
using OrderProcessor.Domain.Entities;

namespace OrderProcessor.Infrastructure.Data;

public class OrderProcessorDbContext : DbContext
{
  public OrderProcessorDbContext(DbContextOptions<OrderProcessorDbContext> options) : base(options)
  {
  }

  public DbSet<Order> Orders { get; set; }
  public DbSet<OrderItem> OrderItems { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<Order>(entity =>
    {
      entity.HasKey(e => e.Id);
      entity.Property(e => e.EmailContent);
      entity.Property(e => e.AttachmentEml);
      entity.Property(e => e.ReceivedDate).IsRequired();
    });

    modelBuilder.Entity<OrderItem>(entity =>
    {
      entity.HasKey(e => e.Id);
      entity.Property(e => e.ProductName).IsRequired();
      entity.Property(e => e.Quantity).IsRequired();
      entity.Property(e => e.Price).IsRequired();
      entity.HasOne(e => e.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(e => e.OrderId);
    });
  }
}