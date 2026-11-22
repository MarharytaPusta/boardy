using Boardy.Domain.Shared;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Boardy.Application.GameCatalog.Statistics;

public class GetGameStatisticsUseCase
{
    private readonly IGameStatisticsRepository _statisticsRepository;
    private readonly ILogger<GetGameStatisticsUseCase> _logger;

    public GetGameStatisticsUseCase(
        IGameStatisticsRepository statisticsRepository,
        ILogger<GetGameStatisticsUseCase> logger)
    {
        _statisticsRepository = statisticsRepository;
        _logger = logger;
    }

    public async Task<Result<GameStatisticsDto>> ExecuteAsync(
        Guid gameId,
        int topResultsLimit = 3,
        CancellationToken cancellationToken = default)
    {
        if (gameId == Guid.Empty)
        {
            _logger.LogWarning("Validation failed: GameId is empty.");
            return Result<GameStatisticsDto>.Failure("Некоректний ідентифікатор гри.");
        }

        if (topResultsLimit <= 0)
        {
            topResultsLimit = 3;
        }

        var statistics = await _statisticsRepository.GetByGameIdAsync(
            gameId,
            topResultsLimit,
            cancellationToken);

        if (statistics is null)
        {
            _logger.LogWarning("Game with ID {GameId} was not found when loading statistics.", gameId);
            return Result<GameStatisticsDto>.Failure("Гру не знайдено.");
        }

        _logger.LogInformation(
            "Loaded statistics for game {GameId}: {MatchesCount} matches, {PlayersCount} players.",
            gameId,
            statistics.TotalPlayedMatches,
            statistics.Players.Count);

        return Result<GameStatisticsDto>.Success(statistics);
    }
}