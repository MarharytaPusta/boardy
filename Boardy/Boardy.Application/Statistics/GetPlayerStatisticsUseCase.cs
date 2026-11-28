using Boardy.Application.GameCatalog.Game;
using Boardy.Application.Player;
using Boardy.Application.Scoring;
using Boardy.Domain.Shared;
using Microsoft.Extensions.Logging;

namespace Boardy.Application.Statistics;

public class GetPlayerStatisticsUseCase
{
    private readonly IPlayerRepository _playerRepository;
    private readonly IPlayerStatisticsRepository _playerStatisticsRepository;
    private readonly ILogger<GetPlayerStatisticsUseCase> _logger;

    public GetPlayerStatisticsUseCase(
        IPlayerRepository playerRepository,
        IPlayerStatisticsRepository playerStatisticsRepository,
        ILogger<GetPlayerStatisticsUseCase> logger)
    {
        _playerRepository = playerRepository;
        _playerStatisticsRepository = playerStatisticsRepository;
        _logger = logger;
    }

    public async Task<Result<GetPlayerStatisticsDto>> ExecuteAsync(Guid playerId, CancellationToken cancellationToken = default)
    {

        if (playerId == Guid.Empty)
        {
            _logger.LogWarning("Validation failed: PlayerId is empty.");
            return Result<GetPlayerStatisticsDto>.Failure("Некоректний ідентифікатор гравця.");
        }

        var player = await _playerRepository.GetByIdAsync(playerId);
        if (player is null || player.IsDeleted)
        {
            _logger.LogWarning("Player with ID {PlayerId} was not found when loading statistics.", playerId);
            return Result<GetPlayerStatisticsDto>.Failure("Гравця не знайдено.");
        }

        var games = await _playerStatisticsRepository.GetPlayerGamesStatisticsAsync(playerId, cancellationToken);

        var statistics = new GetPlayerStatisticsDto(
            player.Id,
            player.Name,
            PlayedGamesCount: games.Count,
            PlayedMatchesCount: games.Sum(game => game.PlayedMatchesCount),
            WinsCount: games.Sum(game => game.WinsCount),
            games);

        _logger.LogInformation("Loaded statistics for player with ID {PlayerId}.", playerId);
        return Result<GetPlayerStatisticsDto>.Success(statistics);
    }

}
