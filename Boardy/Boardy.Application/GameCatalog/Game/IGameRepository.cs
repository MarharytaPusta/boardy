using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boardy.Application.GameCatalog.Game;

public interface IGameRepository
{
    Task AddAsync(Domain.Entities.Game game);
    Task<IEnumerable<Domain.Entities.Game>> GetAllAsync();

    Task<IEnumerable<Domain.Entities.Game>> GetFilteredAsync(string? searchQuery, int? playersCount);
}
