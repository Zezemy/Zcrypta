using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Zcrypta.Entities.Dtos;
using Zcrypta.Entities.Models;
using Zcrypta.Models;

namespace Zcrypta.Context
{
    public class ApplicationDbContext : IdentityDbContext<User> 
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<SignalStrategy> SignalStrategies { get; set; }
        public DbSet<TradingPair> TradingPairs { get; set; }
        public DbSet<UserSignalStrategy> UserSignalStrategies { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<SignalStrategy>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__SignalSt__3214EC0711B64996");

                entity.Property(e => e.CreateDate).HasColumnType("datetime");
                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);
                entity.Property(e => e.UpdateDate).HasColumnType("datetime");
                entity.Property(e => e.UpdatedBy)
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<TradingPair>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__TradingP__3214EC076E5471E1");

                entity.Property(e => e.Base)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false);
                entity.Property(e => e.Quote)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<UserSignalStrategy>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__UserSign__3214EC07929FD2F1");

                entity.Property(e => e.UserId)
                    .IsRequired()
                    .HasMaxLength(450);

                entity.HasOne(d => d.Strategy).WithMany(p => p.UserSignalStrategies)
                    .HasForeignKey(d => d.StrategyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserSignalStrategies_SignalStrategies");

                entity.HasOne(d => d.TradingPair).WithMany(p => p.UserSignalStrategies)
                    .HasForeignKey(d => d.TradingPairId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UserSignalStrategies_TradingPairs");
            });
        }
    }
}

