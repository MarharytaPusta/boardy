using Boardy.Application.GameCatalog.Game;
using Boardy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boardy.Infrastructure.Repositories;

public class GameRepository : IGameRepository
{
    private readonly BoardyDbContext _context;

    public GameRepository(BoardyDbContext context)
    {
        _context = context;
    }
    public async Task AddAsync(Game game)
    {
        await _context.Games.AddAsync(game);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Game>> GetAllAsync()
    {
        return await _context.Games.ToListAsync();
    }

    public async Task<IEnumerable<Game>> GetFilteredAsync(string? searchQuery, int? playersCount)
    {
        var query = _context.Games.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            query = query.Where(g => g.Name.Contains(searchQuery));
        }

        if (playersCount.HasValue)
        {
            query = query.Where(g => g.MinPlayers <= playersCount.Value && g.MaxPlayers >= playersCount.Value);
        }

        return await query
            .Include(g => g.ScoreTemplates)
            .ThenInclude(t => t.Criteria)
            .ToListAsync();
    }

    public async Task<Game?> GetByIdAsync(Guid id)
    {
        return await _context.Games
            .Include(g => g.ScoreTemplates)
            .ThenInclude(t => t.Criteria)
            .FirstOrDefaultAsync(g => g.Id == id);
    }

    public async Task<string?> GetNameByScoreTemplateIdAsync(Guid scoreTemplateId)
    {
        return await _context.Games
            .Where(game => game.ScoreTemplates.Any(template => template.Id == scoreTemplateId))
            .Select(game => game.Name)
            .FirstOrDefaultAsync();
    }

    public async Task DeleteAsync(Game game)
    {
        _context.Games.Remove(game);
        await _context.SaveChangesAsync();
    }


    public async Task UpdateAsync(Game game)
    {
        if (_context.Entry(game).State == EntityState.Detached)
        {
            _context.Games.Update(game);
        }

        foreach (var template in game.ScoreTemplates)
        {
            var templateEntry = _context.Entry(template);

            if (templateEntry.State == EntityState.Modified)
            {
                bool templateExists = await _context.Set<Boardy.Domain.Entities.ScoreTemplate>()
                                                    .AsNoTracking()
                                                    .AnyAsync(t => t.Id == template.Id);
                if (!templateExists)
                {
                    templateEntry.State = EntityState.Added;
                }
            }

            foreach (var criterion in template.Criteria)
            {
                var criterionEntry = _context.Entry(criterion);
                if (criterionEntry.State == EntityState.Modified || criterionEntry.State == EntityState.Detached)
                {
                    bool criterionExists = await _context.Set<Boardy.Domain.Entities.ScoreCriterion>()
                                                         .AsNoTracking()
                                                         .AnyAsync(c => c.Id == criterion.Id);
                    if (!criterionExists)
                    {
                        criterionEntry.State = EntityState.Added;
                    }
                }
            }
        }

        var orphanedCriteria = _context.ChangeTracker.Entries<Boardy.Domain.Entities.ScoreCriterion>()
            .Where(e => e.State == EntityState.Modified &&
                        e.Properties.Any(p => p.Metadata.IsForeignKey() && p.CurrentValue == null))
            .ToList();

        foreach (var entry in orphanedCriteria)
        {
            entry.State = EntityState.Deleted;
        }

        await _context.SaveChangesAsync();
    }
} 
