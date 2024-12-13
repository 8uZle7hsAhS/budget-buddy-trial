using Microsoft.EntityFrameworkCore;
using budget_buddy_trial.Models;

namespace budget_buddy_trial.Data
{
    public class BudgetDbContext : DbContext
    {
        public BudgetDbContext(DbContextOptions<BudgetDbContext> options)
        : base(options)
        {
        }

        // Existing DbSets
        public DbSet<LogModel> Users { get; set; }
        public DbSet<BudgetCategory> BudgetCategories { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        // New DbSets for Feedback and Announcements
        public DbSet<UserFeedback> UserFeedbacks { get; set; }
        public DbSet<AdminAnnouncement> AdminAnnouncements { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Existing configurations
            modelBuilder.Entity<BudgetCategory>()
                .Property(b => b.Status)
                .HasConversion<string>();

            // Configure keyless entities
            modelBuilder.Entity<DashboardViewModel>().HasNoKey();

            // Configure relationships for UserFeedback
            modelBuilder.Entity<UserFeedback>()
                .HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure UserFeedback status conversion
            modelBuilder.Entity<UserFeedback>()
                .Property(f => f.Status)
                .HasConversion<string>();
        }
    }
}