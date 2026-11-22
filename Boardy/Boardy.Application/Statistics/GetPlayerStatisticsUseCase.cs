using Boardy.Application.GameCatalog.Game;
using Boardy.Application.Player;
using Boardy.Application.Scoring;
using Boardy.Domain.Shared;
using Microsoft.Extensions.Logging;

namespace Boardy.Application.Statistics;

public class GetPlayerStatisticsUseCase
{
    private readonly IPlayerRepository _playerRepository;
    private readonly IMatchRepository _matchRepository;
    private readonly IGameRepository _gameRepository;
    private readonly ILogger<GetPlayerStatisticsUseCase> _logger;

    public GetPlayerStatisticsUseCase(
        IPlayerRepository playerRepository,
        IMatchRepository matchRepository,
        IGameRepository gameRepository,
        ILogger<GetPlayerStatisticsUseCase> logger)
    {
        _playerRepository = playerRepository;
        _matchRepository = matchRepository;
        _gameRepository = gameRepository;
        _logger = logger;
    }

    public async Task<Result<GetPlayerStatisticsDto>> ExecuteAsync(Guid playerId)
    {
        var player = await _playerRepository.GetByIdAsync(playerId);
        if (player is null || player.IsDeleted)
        {
            return Result<GetPlayerStatisticsDto>.Failure("Гравця не знайдено.");
        }

        var matches = (await _matchRepository.GetCompletedByPlayerIdsAsync(new[] { playerId })).ToList();
        var games = await GetGameStatisticsAsync(matches, playerId);
        var statistics = new GetPlayerStatisticsDto(
            player.Id,
            player.Name,
            player.PlayedGamesCount,
            player.PlayedMatchesCount,
            player.WinsCount,
            games);

        _logger.LogInformation("Loaded statistics for player with ID {PlayerId}.", playerId);
        return Result<GetPlayerStatisticsDto>.Success(statistics);
    }

    private async Task<IReadOnlyCollection<PlayerGameStatisticsDto>> GetGameStatisticsAsync(
        IEnumerable<Domain.Entities.Match> matches,
        Guid playerId)
    {
        var playedMatches = matches.ToList();
        if (playedMatches.Count == 0)
        {
            return Array.Empty<PlayerGameStatisticsDto>();
        }

        var scoreTemplateIds = playedMatches
            .Select(match => match.ScoreTemplateId)
            .Distinct()
            .ToList();
        var gameIdsByScoreTemplateId = await _gameRepository.GetGameIdsByScoreTemplateIdsAsync(scoreTemplateIds);
        var gameNamesById = new Dictionary<Guid, string>();

        foreach (var gameId in gameIdsByScoreTemplateId.Values.Distinct())
        {
            var game = await _gameRepository.GetByIdAsync(gameId);
            gameNamesById[gameId] = game?.Name
                ?? "Невідома гра";
        }

        return playedMatches
            .GroupBy(match => gameIdsByScoreTemplateId.TryGetValue(match.ScoreTemplateId, out var gameId)
                ? (IsKnownGame: true, Id: gameId)
                : (IsKnownGame: false, Id: match.ScoreTemplateId))
            .Select(matchesByGame =>
            {
                var gameName = matchesByGame.Key.IsKnownGame
                    ? gameNamesById[matchesByGame.Key.Id]
                    : "Невідома гра";
                var gameMatches = matchesByGame.ToList();
                var winsCount = gameMatches.Count(match => match.Participants.Any(
                    participant => participant.PlayerId == playerId && participant.IsWinner));

                return new PlayerGameStatisticsDto(
                    gameName,
                    gameMatches.Count,
                    winsCount);
            })
            .OrderBy(item => item.GameName)
            .ToList();
    }
}
