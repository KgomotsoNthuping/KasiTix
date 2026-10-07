using KasiTix.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KasiTix.Infrastructure.Data;

public sealed class KasiTixDbContext : DbContext
{
    public KasiTixDbContext( DbContextOptions<KasiTixDbContext> options) : base(options) {  }

    public DbSet<Event> Events => Set<Event>();

    public DbSet<TicketType> TicketTypes => Set<TicketType>();

    public DbSet<Order> Orders =>  Set<Order>();

    public DbSet<OrderLine> OrderLines =>  Set<OrderLine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureEvents(modelBuilder);
        ConfigureTicketTypes(modelBuilder);
        ConfigureOrders(modelBuilder);
        ConfigureOrderLines(modelBuilder);
    }

    private static void ConfigureEvents(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Event>();

        entity.HasKey(e => e.Id);

        entity.Property(e => e.Id)
            .ValueGeneratedNever();

        entity.Property(e => e.Name)
            .HasMaxLength(120)
            .IsRequired();

        entity.Property(e => e.Venue)
            .IsRequired();

        entity.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        entity.HasMany(e => e.TicketTypes)
            .WithOne()
            .HasForeignKey(ticket => ticket.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.Navigation(e => e.TicketTypes)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }

    private static void ConfigureTicketTypes(ModelBuilder modelBuilder)
    {
        var entity =  modelBuilder.Entity<TicketType>();

        entity.HasKey(ticket => ticket.Id);

        entity.Property(ticket => ticket.Id)
            .ValueGeneratedNever();

        entity.Property(ticket => ticket.Name)
            .IsRequired();

        entity.Property(ticket => ticket.Price)
            .HasPrecision(18, 2);

        entity.HasIndex(ticket => new
            {
                ticket.EventId,
                ticket.Name
            })
            .IsUnique()
            .HasDatabaseName("UX_TicketTypes_Event_Name");

        entity.ToTable(table =>
        {
            table.HasCheckConstraint("CK_TicketTypes_Price_NonNegative",
                "\"Price\" >= 0");

            table.HasCheckConstraint("CK_TicketTypes_Sold_Capacity",
                "\"Sold\" >= 0 AND \"Sold\" <= \"Capacity\"");
        });

        entity.Property(ticket => ticket.Version)
            .IsRowVersion();
    }

    private static void ConfigureOrders(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Order>();

        entity.HasKey(order => order.Id);

        entity.Property(order => order.Id)
            .ValueGeneratedNever();

        entity.Property(order => order.BuyerEmail)
            .IsRequired();

        entity.Property(order => order.IdempotencyKey)
            .IsRequired();

        entity.Property(order => order.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        entity.HasIndex(order => order.IdempotencyKey)
            .IsUnique()
            .HasDatabaseName( "UX_Orders_IdempotencyKey");

        entity.HasIndex(order => new
            {
                order.EventId,
                order.BuyerEmail
            })
            .IsUnique()
            .HasFilter( "\"Status\" = 'Confirmed'")
            .HasDatabaseName("UX_Orders_Event_Buyer_Confirmed");

        entity.HasOne<Event>()
            .WithMany()
            .HasForeignKey(order => order.EventId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasMany(order => order.Lines)
            .WithOne()
            .HasForeignKey(line => line.OrderId)
            .OnDelete( DeleteBehavior.Cascade);

        entity.Navigation(order => order.Lines)
            .UsePropertyAccessMode( PropertyAccessMode.Field);
    }

    private static void ConfigureOrderLines(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<OrderLine>();

        entity.HasKey(line => line.Id);

        entity.Property(line => line.Id)
            .ValueGeneratedNever();

        entity.Property(line =>  line.UnitPrice)
            .HasPrecision(18, 2);

        entity.ToTable(table =>
        {
            table.HasCheckConstraint( "CK_OrderLines_Quantity",  "\"Quantity\" BETWEEN 1 AND 10");
        });

        entity.HasOne<TicketType>()
            .WithMany()
            .HasForeignKey(line => line.TicketTypeId)
            .OnDelete( DeleteBehavior.Restrict);
    }
}