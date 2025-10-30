using ExpenseSplitter.Web.Data;
using ExpenseSplitter.Web.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ExpenseSplitter.Web.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Trip> Trips => Set<Trip>();
        public DbSet<TripMember> TripMembers => Set<TripMember>();
        public DbSet<Expense> Expenses => Set<Expense>();
        public DbSet<ExpenseSplit> ExpenseSplits => Set<ExpenseSplit>();
        public DbSet<Settlement> Settlements => Set<Settlement>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // MySQL index length compatibility for Identity (utf8mb4 => 4 bytes/char, 191 * 4 = 764 < 767)
            builder.Entity<ApplicationUser>(b =>
            {
                b.ToTable("ES1485943_AspNetUsers");
                b.Property(u => u.UserName).HasMaxLength(191);
                b.Property(u => u.NormalizedUserName).HasMaxLength(191);
                b.Property(u => u.Email).HasMaxLength(191);
                b.Property(u => u.NormalizedEmail).HasMaxLength(191);
            });
            builder.Entity<Microsoft.AspNetCore.Identity.IdentityRole>(b =>
            {
                b.ToTable("ES1485943_AspNetRoles");
                b.Property(r => r.Name).HasMaxLength(191);
                b.Property(r => r.NormalizedName).HasMaxLength(191);
            });

            builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserLogin<string>>(b =>
            {
                b.ToTable("ES1485943_AspNetUserLogins");
                b.Property(l => l.LoginProvider).HasMaxLength(191);
                b.Property(l => l.ProviderKey).HasMaxLength(191);
            });
            builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserToken<string>>(b =>
            {
                b.ToTable("ES1485943_AspNetUserTokens");
                b.Property(t => t.LoginProvider).HasMaxLength(191);
                b.Property(t => t.Name).HasMaxLength(191);
            });
            builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserClaim<string>>(b =>
            {
                b.ToTable("ES1485943_AspNetUserClaims");
            });
            builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserRole<string>>(b =>
            {
                b.ToTable("ES1485943_AspNetUserRoles");
            });
            builder.Entity<Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>>(b =>
            {
                b.ToTable("ES1485943_AspNetRoleClaims");
            });

            builder.Entity<Trip>(e =>
            {
                e.ToTable("ES1485943_Trips");
                e.HasKey(x => x.TripId);
                e.HasIndex(x => x.ShareableLink).IsUnique();
                e.Property(x => x.TotalBudget).HasPrecision(18, 2);
                e.HasOne<ApplicationUser>()
                    .WithMany()
                    .HasForeignKey(x => x.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<TripMember>(e =>
            {
                e.ToTable("ES1485943_TripMembers");
                e.HasKey(x => x.TripMemberId);
                e.Property(x => x.PersonalBudget).HasPrecision(18, 2);
                e.Property(x => x.CurrentBalance).HasPrecision(18, 2);
                e.HasOne(x => x.Trip)
                    .WithMany(x => x.Members)
                    .HasForeignKey(x => x.TripId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.HasOne<ApplicationUser>()
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
                e.HasIndex(x => new { x.TripId, x.UserId }).IsUnique();
            });

            builder.Entity<Expense>(e =>
            {
                e.ToTable("ES1485943_Expenses");
                e.HasKey(x => x.ExpenseId);
                e.Property(x => x.TotalAmount).HasPrecision(18, 2);
                e.Property(x => x.OriginalAmount).HasPrecision(18, 2);
                e.Property(x => x.ExchangeRate).HasPrecision(18, 6);
                e.HasOne(x => x.Trip)
                    .WithMany(x => x.Expenses)
                    .HasForeignKey(x => x.TripId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.HasOne<ApplicationUser>()
                    .WithMany()
                    .HasForeignKey(x => x.PaidBy)
                    .OnDelete(DeleteBehavior.Restrict);
                e.HasIndex(x => new { x.TripId, x.ExpenseDate });
            });

            builder.Entity<ExpenseSplit>(e =>
            {
                e.ToTable("ES1485943_ExpenseSplits");
                e.HasKey(x => x.SplitId);
                e.Property(x => x.AmountOwed).HasPrecision(18, 2);
                e.Property(x => x.AmountPaid).HasPrecision(18, 2);
                e.HasOne(x => x.Expense)
                    .WithMany(x => x.Splits)
                    .HasForeignKey(x => x.ExpenseId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.HasOne<ApplicationUser>()
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
                e.HasIndex(x => new { x.ExpenseId, x.UserId }).IsUnique();
            });

            builder.Entity<Settlement>(e =>
            {
                e.ToTable("ES1485943_Settlements");
                e.HasKey(x => x.SettlementId);
                e.Property(x => x.Amount).HasPrecision(18, 2);
                e.HasOne<Trip>()
                    .WithMany()
                    .HasForeignKey(x => x.TripId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.HasOne<ApplicationUser>()
                    .WithMany()
                    .HasForeignKey(x => x.FromUserId)
                    .OnDelete(DeleteBehavior.Restrict);
                e.HasOne<ApplicationUser>()
                    .WithMany()
                    .HasForeignKey(x => x.ToUserId)
                    .OnDelete(DeleteBehavior.Restrict);
                e.HasIndex(x => new { x.TripId, x.FromUserId, x.ToUserId });
            });
        }
    }
}
