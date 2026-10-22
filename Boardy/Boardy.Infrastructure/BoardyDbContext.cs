using Boardy.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Boardy.Infrastructure
{
    public class BoardyDbContext : DbContext
    {
        public BoardyDbContext(DbContextOptions<BoardyDbContext> options) : base(options) { }

        public DbSet<Player> Players { get; set; }

        public DbSet<ScoreTemplate> ScoreTemplates { get; set; }
        public DbSet<ScoreCriterion> ScoreCriteria { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ScoreTemplate>()
                .HasMany(t => t.Criteria)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
