using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Boardy.Application.Statistics;

public interface IPlayerStatisticsRepository
{
    Task<IReadOnlyList<GetOverallRatingDto>> GetOverallRatingAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PlayerGameStatisticsDto>> GetPlayerGamesStatisticsAsync(
        Guid playerId,
        CancellationToken cancellationToken = default);
}