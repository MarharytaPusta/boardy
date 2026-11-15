using Boardy.Application.GameCatalog.Game;
using Boardy.Application.Player;
using Boardy.Domain.Shared;
using Microsoft.Extensions.Logging;

namespace Boardy.Application.Scoring;

public class GetMatchHistoryUseCase
{
    private readonly IMatchRepository _matchRepository;
    private readonly IPlayerRepository _playerRepository;
    private readonly IGameRepository _gameRepository;
    private readonly ILogger<GetMatchHistoryUseCase> _logger;

    public GetMatchHistoryUseCase(
        IMatchRepository matchRepository,
        IPlayerRepository playerRepository,
        IGameRepository gameRepository,
        ILogger<GetMatchHistoryUseCase> logger)
    {
        _matchRepository = matchRepository;
        _playerRepository = playerRepository;
        _gameRepository = gameRepository;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<GetMatchHistoryDto>>> ExecuteAsync()
    {
        var matches = (await _matchRepository.GetCompletedAsync()).ToList();
        var playerIds = matches
            .SelectMany(match => match.Participants)
            .Select(participant => participant.PlayerId)
            .Distinct()
            .ToList();
        var players = await _playerRepository.GetByIdsAsync(playerIds);
        var playerNames = players.ToDictionary(player => player.Id, player => player.Name);
        var historyItems = new List<GetMatchHistoryDto>();

        foreach (var match in matches)
        {
            var gameName = await _gameRepository.GetNameByScoreTemplateIdAsync(match.ScoreTemplateId)
                ?? "Невідома гра";
            var participants = match.Participants
                .Select(participant => GetPlayerName(participant.PlayerId, playerNames));
            var winners = match.Participants
                .Where(participant => participant.IsWinner)
                .Select(participant => GetPlayerName(participant.PlayerId, playerNames));

            historyItems.Add(new GetMatchHistoryDto(
                match.Id,
                match.Date,
                gameName,
                string.Join(", ", participants),
                string.Join(", ", winners)));
        }

        _logger.LogInformation("Loaded {MatchCount} completed matches for history.", historyItems.Count);
        return Result<IEnumerable<GetMatchHistoryDto>>.Success(historyItems);
    }

    private static string GetPlayerName(Guid playerId, IReadOnlyDictionary<Guid, string> playerNames)
    {
        return playerNames.TryGetValue(playerId, out var playerName)
            ? playerName
            : "Невідомий гравець";
    }
}
