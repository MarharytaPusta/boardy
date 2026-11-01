using Boardy.Application.GameCatalog.ScoreTemplate;
using Microsoft.EntityFrameworkCore;
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

        public async Task<Domain.Entities.ScoreTemplate?> GetByIdAsync(Guid id)
        {
            return await _context.ScoreTemplates
                .Include(template => template.Criteria)
                .FirstOrDefaultAsync(template => template.Id == id);
        }
    }
}