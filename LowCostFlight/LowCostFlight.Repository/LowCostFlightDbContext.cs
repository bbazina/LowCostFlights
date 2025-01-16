using LowCostFlight.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace LowCostFlight.Repository
{
    public class LowCostFlightDbContext : DbContext
    {
        public LowCostFlightDbContext(DbContextOptions<LowCostFlightDbContext> options)
            : base(options)
        {
        }

        public DbSet<Flight> Flights { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Flight>(eb =>
            {
                eb.ToTable(nameof(Flight));

                eb.HasKey(f => f.Id);

                eb.Property(f => f.Id).ValueGeneratedOnAdd();
            });
        }
    }
}
