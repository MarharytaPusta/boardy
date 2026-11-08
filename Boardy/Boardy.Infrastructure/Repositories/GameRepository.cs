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

    public async Task UpdateAsync(Game game)
    {
        if (_context.Entry(game).State == EntityState.Detached)
        {
            _context.Games.Update(game);
        }

        foreach (var template in game.ScoreTemplates)
        {
            var entry = _context.Entry(template);

            if (entry.State == EntityState.Modified)
            {
                bool existsInDb = await _context.Set<Boardy.Domain.Entities.ScoreTemplate>()
                                                .AsNoTracking()
                                                .AnyAsync(t => t.Id == template.Id);

                if (!existsInDb)
                {
                    entry.State = EntityState.Added;

                    foreach (var criterion in template.Criteria)
                    {
                        _context.Entry(criterion).State = EntityState.Added;
                    }
                }
            }
        }

        await _context.SaveChangesAsync();
    }
} 
