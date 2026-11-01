using Boardy.Domain.Enums;
using System;
using System.Collections.Generic;

namespace Boardy.Application.Scoring;

public record GetMatchResultsRequest(Guid MatchId);

public record MatchResultsDto(
    Guid Id,
    DateTime Date,
    MatchStatus Status,
    IEnumerable<MatchParticipantResultDto> Participants
);

public record MatchParticipantResultDto(
    Guid Id,
    Guid PlayerId,
    string PlayerName,
    int TotalScore,
    bool IsWinner
);
