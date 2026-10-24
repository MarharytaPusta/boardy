using Boardy.Application.GameCatalog.Catalog;
using Boardy.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Boardy.Infrastructure.Repositories;

public class GameCatalogRepository : IGameCatalogRepository
{
    private readonly BoardyDbContext _context;

    public GameCatalogRepository(BoardyDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Game>> GetAllAsync()
    {
        return await _context.Games
            .AsNoTracking()
            .ToListAsync();
    }
}
