using System;
using System.Collections.Generic;

namespace Boardy.Application.Statistics;

public record GameStatisticsDto(
    Guid GameId,
    string GameName,
    int TotalPlayedMatches,
    IReadOnlyList<TopScoreDto> TopResults,
    IReadOnlyList<GamePlayerStatDto> Players
);

public record TopScoreDto(
    Guid PlayerId,
    string PlayerName,
    bool IsPlayerDeleted,
    int Score,
    DateTime MatchDate,
    string ScoreTemplateName
);

public record GamePlayerStatDto(
    Guid PlayerId,
    string PlayerName,
    bool IsPlayerDeleted,
    int MatchesPlayedInGame,
    int WinsInGame,
    int BestScoreInGame
);