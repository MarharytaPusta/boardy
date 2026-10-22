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
}
