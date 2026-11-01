using Boardy.Domain.Enums;
using System;
using System.Collections.Generic;

namespace Boardy.Application.Scoring;

public record GetMatchResultsRequest(Guid MatchId);

public record MatchResultsDto(
    Guid Id,
    DateTime Date,
    MatchStatus Status,
    string GameName,
    IEnumerable<ScoreCriterionDto> Criteria,
    IEnumerable<MatchParticipantResultDto> Participants
);

public record ScoreCriterionDto(Guid Id, string Name, bool IsNegative);

public record MatchParticipantResultDto(
    Guid Id,
    Guid PlayerId,
    string PlayerName,
    int TotalScore,
    bool IsWinner,
    IEnumerable<CriterionResultDto> CriterionResults
);

public record CriterionResultDto(Guid CriterionId, int Score);