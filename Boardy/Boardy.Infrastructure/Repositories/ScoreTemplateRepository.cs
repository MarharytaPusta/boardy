using Boardy.Application.GameCatalog.ScoreTemplate;
using System.Threading.Tasks;

namespace Boardy.Infrastructure.Repositories
{
    public class ScoreTemplateRepository : IScoreTemplateRepository
    {
        private readonly BoardyDbContext _context;

        public ScoreTemplateRepository(BoardyDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Domain.Entities.ScoreTemplate template)
        {
            await _context.ScoreTemplates.AddAsync(template);
            await _context.SaveChangesAsync();
        }
    }
}