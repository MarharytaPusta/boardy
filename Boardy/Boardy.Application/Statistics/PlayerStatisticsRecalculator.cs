using Boardy.Application.GameCatalog.Game;
using Boardy.Application.Player;
using Boardy.Application.Scoring;
using Boardy.Domain.Shared;
using Microsoft.Extensions.Logging;

namespace Boardy.Application.Statistics;

public class PlayerStatisticsRecalculator : IPlayerStatisticsRecalculator
{
    private readonly IPlayerRepository _playerRepository;
    private readonly IMatchRepository _matchRepository;
    private readonly IGameRepository _gameRepository;
    private readonly ILogger<PlayerStatisticsRecalculator> _logger;

    public PlayerStatisticsRecalculator(
        IPlayerRepository playerRepository,
        IMatchRepository matchRepository,
        IGameRepository gameRepository,
        ILogger<PlayerStatisticsRecalculator> logger)
    {
        _playerRepository = playerRepository;
        _matchRepository = matchRepository;
        _gameRepository = gameRepository;
        _logger = logger;
    }

    public async Task<Result> RecalculateAsync(IEnumerable<Guid> playerIds)
    {
        var distinctPlayerIds = playerIds.Distinct().ToList();
        var players = (await _playerRepository.GetByIdsAsync(distinctPlayerIds)).ToList();
        if (players.Count != distinctPlayerIds.Count)
        {
            _logger.LogWarning("Statistics recalculation failed because one or more match participants were not found.");
            return Result.Failure("Одного або кількох гравців не знайдено.");
        }

        var completedMatches = (await _matchRepository.GetCompletedByPlayerIdsAsync(distinctPlayerIds)).ToList();
        var scoreTemplateIds = completedMatches
            .Select(match => match.ScoreTemplateId)
            .Distinct()
            .ToList();
        var gameIdsByScoreTemplateId = await _gameRepository.GetGameIdsByScoreTemplateIdsAsync(scoreTemplateIds);

        foreach (var player in players)
        {
            var playerMatches = completedMatches
                .Where(match => match.Participants.Any(participant => participant.PlayerId == player.Id))
                .ToList();
            var playedGamesCount = playerMatches
                .Where(match => gameIdsByScoreTemplateId.ContainsKey(match.ScoreTemplateId))
                .Select(match => gameIdsByScoreTemplateId[match.ScoreTemplateId])
                .Distinct()
                .Count();
            var winsCount = playerMatches.Count(match => match.Participants.Any(
                participant => participant.PlayerId == player.Id && participant.IsWinner));

            player.UpdateStatistics(playedGamesCount, playerMatches.Count, winsCount);
            await _playerRepository.UpdateAsync(player);
        }

        return Result.Success();
    }
}
