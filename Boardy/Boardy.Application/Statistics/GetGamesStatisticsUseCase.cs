using Boardy.Application.GameCatalog.Game;
using Boardy.Application.Scoring;
using Boardy.Domain.Enums;
using Boardy.Domain.Shared;
using Microsoft.Extensions.Logging;

namespace Boardy.Application.GameCatalog.Catalog;

public record GamesStatisticsDto(
    string MostPlayedGame,
    string LeastPlayedGame,
    IReadOnlyDictionary<Guid, int> MatchesCountByGame);

public class GetGamesStatisticsUseCase
{
    private readonly IGameRepository _gameRepository;
    private readonly IMatchRepository _matchRepository;
    private readonly ILogger<GetGamesStatisticsUseCase> _logger;

    public GetGamesStatisticsUseCase(
        IGameRepository gameRepository,
        IMatchRepository matchRepository,
        ILogger<GetGamesStatisticsUseCase> logger)
    {
        _gameRepository = gameRepository;
        _matchRepository = matchRepository;
        _logger = logger;
    }

    public async Task<Result<GamesStatisticsDto>> ExecuteAsync(bool includeInactive = false)
    {
        _logger.LogInformation("Calculating game popularity statistics (IncludeInactive: {IncludeInactive}).", includeInactive);

        var allGames = await _gameRepository.GetAllAsync();
        var matchesCountByGame = await _matchRepository.GetMatchesCountByGameAsync();

        var targetGames = (includeInactive
                ? allGames
                : allGames.Where(g => g.Status == GameStatus.Active))
            .Select(g => new
            {
                g.Id,
                g.Name,
                MatchesCount = matchesCountByGame.GetValueOrDefault(g.Id, 0)
            })
            .ToList();

        var (mostPlayed, leastPlayed) = CalculateExtremes(targetGames.Select(g => (g.Name, g.MatchesCount)).ToList());

        _logger.LogInformation(
            "Calculated game statistics for {GamesCount} games. Most played: {MostPlayed}, Least played: {LeastPlayed}.",
            targetGames.Count,
            mostPlayed,
            leastPlayed);

        return Result<GamesStatisticsDto>.Success(
            new GamesStatisticsDto(mostPlayed, leastPlayed, matchesCountByGame));
    }

    private static (string MostPlayed, string LeastPlayed) CalculateExtremes(
        IReadOnlyCollection<(string Name, int MatchesCount)> games)
    {
        if (games.Count == 0 || games.All(g => g.MatchesCount == 0))
        {
            return ("Немає даних", "Немає даних");
        }

        var mostPlayedGame = games
            .OrderByDescending(g => g.MatchesCount)
            .ThenBy(g => g.Name)
            .First();

        var leastPlayedGame = games
            .OrderBy(g => g.MatchesCount)
            .ThenBy(g => g.Name)
            .First();

        return (
            $"{mostPlayedGame.Name} ({mostPlayedGame.MatchesCount})",
            $"{leastPlayedGame.Name} ({leastPlayedGame.MatchesCount})");
    }
}