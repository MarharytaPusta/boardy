namespace Boardy.Application.Statistics;

public record GetPlayerStatisticsDto(
    Guid PlayerId,
    string PlayerName,
    int PlayedGamesCount,
    int PlayedMatchesCount,
    int WinsCount,
    IReadOnlyCollection<PlayerGameStatisticsDto> Games);

public record PlayerGameStatisticsDto(
    string GameName,
    int PlayedMatchesCount,
    int WinsCount);
