using System;
using System.Threading.Tasks;

namespace Boardy.Application.Player
{
    public interface IPlayerRepository
    {
        Task AddAsync(Domain.Entities.Player player);
        Task<Domain.Entities.Player?> GetByIdAsync(Guid playerId);
        Task UpdateAsync(Domain.Entities.Player player);
    }
}
