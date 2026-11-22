namespace Boardy.Application.Statistics;

public record GetOverallRatingDto(
    Guid PlayerId,
    string PlayerName,
    int PlayedGamesCount,
    int PlayedMatchesCount,
    int WinsCount);
