using DigiPay.Wallet.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DigiPay.Wallet.Api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Models.Wallet> Wallets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Models.Wallet>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Balance).HasColumnType("decimal(18,2)");
                entity.HasIndex(e => e.UserId).IsUnique();
            });

            base.OnModelCreating(modelBuilder);
        }
    }
} 