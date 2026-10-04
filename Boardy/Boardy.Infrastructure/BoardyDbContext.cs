using Boardy.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Boardy.Infrastructure
{
    public class BoardyDbContext : DbContext
    {
        public BoardyDbContext(DbContextOptions<BoardyDbContext> options) : base(options) { }

        public DbSet<Result> Results { get; set; }
    }
}
