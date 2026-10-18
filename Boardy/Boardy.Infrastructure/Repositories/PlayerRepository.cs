using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Boardy.Application.Player;
using Boardy.Domain.Entities;
using Microsoft.EntityFrameworkCore;

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

        public async Task<IEnumerable<Domain.Entities.Player>> GetAllAsync()
        {
            return await _context.Players.ToListAsync();
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

        public async Task DeleteAsync(Domain.Entities.Player player)
        {
            _context.Players.Remove(player);
            await _context.SaveChangesAsync();
        }
    }
}
