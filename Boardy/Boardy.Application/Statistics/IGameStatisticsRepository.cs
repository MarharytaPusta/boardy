using System;
using System.Threading;
using System.Threading.Tasks;

namespace Boardy.Application.Statistics;

public interface IGameStatisticsRepository
{
    Task<GameStatisticsDto?> GetByGameIdAsync(
        Guid gameId,
        int topResultsLimit = 3,
        CancellationToken cancellationToken = default);
}