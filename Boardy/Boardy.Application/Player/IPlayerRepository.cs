using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Boardy.Application.Player
{
    public interface IPlayerRepository
    {
        Task AddAsync(Domain.Entities.Player player);
        Task<IEnumerable<Domain.Entities.Player>> GetAllAsync();
        Task<IEnumerable<Domain.Entities.Player>> GetByIdsAsync(IEnumerable<Guid> playerIds);
        Task<Domain.Entities.Player?> GetByIdAsync(Guid playerId);
        Task UpdateAsync(Domain.Entities.Player player);
        Task DeleteAsync(Domain.Entities.Player player);
    }
}
