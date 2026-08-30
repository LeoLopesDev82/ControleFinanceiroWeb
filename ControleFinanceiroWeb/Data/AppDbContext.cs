using Microsoft.EntityFrameworkCore;
using ControleFinanceiroWeb.Models.Entities;

namespace ControleFinanceiroWeb.Data
{
    // Application Entity Framework database context.
    public class AppDbContext : DbContext
    {
        // Initializes a new instance of AppDbContext with database connection options.
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Database tables mappings for Statements, StatementTypes and Categories.
        public DbSet<Statement> Statement { get; set; }
        public DbSet<StatementTypes> StatementTypes { get; set; }
        public DbSet<Category> Categories { get; set; }

        public DbSet<AppSecurity> AppSecurity { get; set; }

        // Configures model relationships and dynamically registers all entity classes from the Entities namespace.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var entityTypes = typeof(Statement).Assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.Namespace == "ControleFinanceiroWeb.Models.Entities");

            foreach (var type in entityTypes)
            {
                modelBuilder.Entity(type);
            }

            modelBuilder.Entity<Category>()
                .Property(c => c.EntryType)
                .HasConversion(
                    v => (char)v,
                    v => (CategoryType)v);
        }
    }
}