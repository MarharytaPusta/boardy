using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boardy.Application.GameCatalog.Game;

public interface IGameRepository
{
    Task AddAsync(Domain.Entities.Game game);
}
