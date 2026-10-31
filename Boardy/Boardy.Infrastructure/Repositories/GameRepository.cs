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

        return await query.ToListAsync();
    }

}
