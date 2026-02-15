using DevBoard.Models;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using DevBoard.Data;

namespace DevBoard
{
    [DbConfigurationType(typeof(DevBoard.Data.SQLiteConfiguration))]
    public class DevBoardContext : DbContext
    {
        public DevBoardContext() : base("name=DevBoardContext")
        {
        }

        public DbSet<Project> Projects { get; set; }
        public DbSet<Module> Modules { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<TicketVote> TicketVotes { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Remove pluralizing table name convention
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            // Project relationships
            modelBuilder.Entity<Project>()
                .HasMany(p => p.Modules)
                .WithRequired(m => m.Project)
                .HasForeignKey(m => m.ProjectId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Project>()
                .HasMany(p => p.Tickets)
                .WithRequired(t => t.Project)
                .HasForeignKey(t => t.ProjectId)
                .WillCascadeOnDelete(true);

            // Module relationships
            modelBuilder.Entity<Module>()
                .HasMany(m => m.Tickets)
                .WithOptional(t => t.Module)
                .HasForeignKey(t => t.ModuleId)
                .WillCascadeOnDelete(false);

            // Ticket relationships
            modelBuilder.Entity<Ticket>()
                .HasMany(t => t.Votes)
                .WithRequired(v => v.Ticket)
                .HasForeignKey(v => v.TicketId)
                .WillCascadeOnDelete(true);

            // TicketVote unique constraint (one vote per user per ticket)
            modelBuilder.Entity<TicketVote>()
                .HasIndex(v => new { v.TicketId, v.UserId })
                .IsUnique();
        }
    }
}
