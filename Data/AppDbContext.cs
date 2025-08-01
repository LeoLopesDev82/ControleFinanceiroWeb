using Microsoft.EntityFrameworkCore;
using ControleFinanceiroWeb.Models.Entities;

namespace ControleFinanceiroWeb.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Statement> Statement { get; set; }
        public DbSet<StatementTypes> StatementTypes { get; set; }
        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Statement>(entity =>
            {
                entity.ToTable("STATEMENT");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.TransactionDate).HasColumnName("TRANSACTION_DATE");
                entity.Property(e => e.DueDate).HasColumnName("DUE_DATE");
                entity.Property(e => e.Amount).HasColumnName("AMOUNT");
                entity.Property(e => e.Description).HasColumnName("DESCRIPTION");
                entity.Property(e => e.EntryId).HasColumnName("ENTRY_ID");
                entity.Property(e => e.StatementTypeId).HasColumnName("STATEMENT_TYPE_ID");
            });

            modelBuilder.Entity<StatementTypes>(entity =>
            {
                entity.ToTable("STATEMENT_TYPES");

                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Description).HasColumnName("DESCRIPTION");
            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("CATEGORY");

                entity.Property(e => e.Id).HasColumnName("ID");
                
                entity.Property(e => e.Description)
                      .HasColumnName("DESCRIPTION")
                      .IsRequired()
                      .HasMaxLength(255);

                entity.Property(e => e.EntryType)
                      .HasColumnName("ENTRY_TYPE")
                      .IsRequired()
                      .HasColumnType("char(1)");
                
                entity.Property(e => e.StatementIdentifiers)
                      .HasColumnName("STATEMENT_IDENTIFIERS")
                      .IsRequired()
                      .HasMaxLength(5000);
            });
        }
    }
}
