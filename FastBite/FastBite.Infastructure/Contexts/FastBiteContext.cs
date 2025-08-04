using FastBite.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace FastBite.Infrastructure.Contexts;

public class FastBiteContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Table> Tables { get; set; }
    public DbSet<Restaurant> Restaurants { get; set; }
    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductTranslation> ProductTranslations { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<AppRole> AppRoles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }

    public FastBiteContext() { }

    public FastBiteContext(DbContextOptions<FastBiteContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Name).IsRequired().HasMaxLength(100);
            entity.Property(u => u.Surname).IsRequired().HasMaxLength(100);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(150);
            entity.Property(u => u.Password).IsRequired();
            entity.Property(u => u.phoneNumber).HasMaxLength(15).IsRequired(false);
            entity.HasIndex(u => u.phoneNumber).IsUnique();
            entity.Property(u => u.PasswordResetCode).IsRequired(false);
            entity.Property(u => u.PasswordResetCodeExpiryTime).IsRequired(false);

            entity.HasMany(u => u.UserRoles)
                .WithOne(ur => ur.User)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(u => u.Orders)
                .WithOne(o => o.User)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasMany(u => u.Reservations)
                .WithOne(r => r.User)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Table>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Capacity).IsRequired();
            entity.Property(t => t.Number).IsRequired();
            entity.HasMany(t => t.Reservations)
                .WithOne(r => r.Table)
                .HasForeignKey(r => r.TableId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Restaurant>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Name).IsRequired().HasMaxLength(200);
        });

        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.HasKey(r => r.Id);

            entity.Property(r => r.ReservationDate)
                .IsRequired()
                .HasConversion(
                    v => v.ToString("yyyy-MM-dd"), 
                    v => DateOnly.Parse(v));

            entity.Property(r => r.ReservationStart)
                .IsRequired()
                .HasConversion(
                    v => v.ToString("HH:mm"),  
                    v => TimeOnly.Parse(v));     

            entity.Property(r => r.ReservationEnd)
                .IsRequired()
                .HasConversion(
                    v => v.ToString("HH:mm"),
                    v => TimeOnly.Parse(v));  

            entity.HasOne(r => r.User)
                .WithMany(u => u.Reservations)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(r => r.Order)
                .WithOne()
                .HasForeignKey<Reservation>(r => r.OrderId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Price).IsRequired();
            entity.Property(p => p.ImageUrl).IsRequired().HasDefaultValue("https://fastbiteblob.blob.core.windows.net/fastbiteproductimages/product_picture_0_600_600.jpg");

            entity.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(p => p.Translations) 
                .WithOne(t => t.Product)
                .HasForeignKey(t => t.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProductTranslation>(entity => 
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.LanguageCode).IsRequired();
            entity.Property(t => t.Name).IsRequired().HasMaxLength(150);
            entity.Property(t => t.Description).HasMaxLength(500);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(o => o.Id);
            entity.Property(o => o.TotalPrice).IsRequired();
            entity.Property(o => o.TableNumber).IsRequired().HasDefaultValue(0); 

            entity.HasMany(o => o.OrderItems)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Quantity).IsRequired();
            entity.Property(e => e.ProductId).IsRequired(false);

            entity.HasOne(e => e.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<AppRole>(entity =>
        {
            entity.HasKey(ar => ar.Id);
            entity.Property(ar => ar.Name).IsRequired().HasMaxLength(100);

            entity.HasMany(ar => ar.UserRoles)
                .WithOne(ur => ur.AppRole)
                .HasForeignKey(ur => ur.RoleId);
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(ur => new { ur.UserId, ur.RoleId });

            entity.HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            entity.HasOne(ur => ur.AppRole)
                .WithMany(ar => ar.UserRoles)
                .HasForeignKey(ur => ur.RoleId);
        });
    }
}