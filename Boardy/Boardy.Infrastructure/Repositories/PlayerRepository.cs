using System;
using System.Threading.Tasks;
using Boardy.Application.Player;
using Boardy.Domain.Entities;

namespace Boardy.Infrastructure.Repositories
{
    public class PlayerRepository : IPlayerRepository
    {
        private readonly BoardyDbContext _context;

        public PlayerRepository(BoardyDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Domain.Entities.Player player)
        {
            await _context.Players.AddAsync(player);
            await _context.SaveChangesAsync();
        }

        public async Task<Domain.Entities.Player?> GetByIdAsync(Guid playerId)
        {
            return await _context.Players.FindAsync(playerId);
        }

        public async Task UpdateAsync(Domain.Entities.Player player)
        {
            _context.Players.Update(player);
            await _context.SaveChangesAsync();
        }
    }
}
