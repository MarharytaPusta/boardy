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
            .ThenInclude(p => p.CriterionResults)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task UpdateAsync(Match match)
    {
        var newCriterionResults = match.Participants
            .SelectMany(participant => participant.CriterionResults)
            .Where(result => _context.Entry(result).State == EntityState.Detached)
            .ToList();

        if (newCriterionResults.Count > 0)
        {
            await _context.MatchCriterionResults.AddRangeAsync(newCriterionResults);
        }

        await _context.SaveChangesAsync();
    }
}
