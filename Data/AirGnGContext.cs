
    using AirGnG.Models;
    using Microsoft.EntityFrameworkCore;

    namespace AirGnG.Data
    {
        public class AirGnGContext : DbContext
        {
            public AirGnGContext(DbContextOptions<AirGnGContext> options)
                : base(options)
            {
            }

            public DbSet<MenuItem> MenuItems { get; set; } = null!;
            public DbSet<Order> Orders { get; set; } = null!;
            public DbSet<OrderItem> OrderItems { get; set; } = null!;

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                // Seed the in-memory database with initial menu items
                modelBuilder.Entity<MenuItem>().HasData(
                    new MenuItem { Id = 1, Name = "Espresso", Price = 2.50f, Description = "Strong black coffee." },
                    new MenuItem { Id = 2, Name = "Latte", Price = 4.00f, Description = "Espresso with steamed milk." },
                    new MenuItem { Id = 3, Name = "Croissant", Price = 3.50f, Description = "Buttery pastry." },
                    new MenuItem { Id = 4, Name = "Pancakes", Price = 6.50f, Description = "Buttermilk pancakes." },
                    new MenuItem { Id = 5, Name = "Omelette", Price = 4.50f, Description = "3 Egg Omelette." },
                    new MenuItem { Id = 6, Name = "Avocado Toast", Price = 7.50f, Description = "Avocado Toast." }
                );

                base.OnModelCreating(modelBuilder);
            }
        }
    }
