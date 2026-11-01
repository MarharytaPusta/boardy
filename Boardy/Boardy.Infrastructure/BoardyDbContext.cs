using Boardy.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Boardy.Infrastructure
{
    public class BoardyDbContext : DbContext
    {
        public BoardyDbContext(DbContextOptions<BoardyDbContext> options) : base(options) { }

        public DbSet<Match> Matches { get; set; }
        public DbSet<MatchParticipant> MatchParticipants { get; set; }
        public DbSet<MatchCriterionResult> MatchCriterionResults { get; set; }

        public DbSet<Player> Players { get; set; }

        public DbSet<ScoreTemplate> ScoreTemplates { get; set; }
        public DbSet<ScoreCriterion> ScoreCriteria { get; set; }

        public DbSet<Game> Games { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ScoreTemplate>()
                .HasMany(t => t.Criteria)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Game>()
                .HasMany(g => g.ScoreTemplates)
                .WithOne()
                .HasForeignKey("GameId")
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Match>(entity =>
            {
                entity.HasKey(m => m.Id);

                entity.HasMany(m => m.Participants)
                      .WithOne()
                      .HasForeignKey(p => p.MatchId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<MatchParticipant>(entity =>
            {
                entity.HasKey(p => p.Id);

                entity.HasOne<Player>()
                      .WithMany()
                      .HasForeignKey(p => p.PlayerId)
                       .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(p => p.CriterionResults)
                      .WithOne()
                      .HasForeignKey(r => r.MatchParticipantId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
