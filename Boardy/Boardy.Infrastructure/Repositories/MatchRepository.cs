using Boardy.Application.Scoring;
using Boardy.Domain.Entities;
using Microsoft.EntityFrameworkCore;


namespace Boardy.Infrastructure.Repositories;

public class MatchRepository : IMatchRepository
{

    private readonly BoardyDbContext _context;

    public MatchRepository(BoardyDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Match match)
    {
        _context.Matches.Add(match);
        await _context.SaveChangesAsync();
    }

    public async Task<Match?> GetByIdAsync(Guid id)
    {
        return await _context.Matches
            .Include(m => m.Participants)
            .FirstOrDefaultAsync(m => m.Id == id);
    }
}
