using System;
using System.Collections.Generic;

namespace Boardy.Application.Scoring;

public record SaveMatchResultsRequest(
    Guid MatchId,
    List<MatchParticipantScoreRequest> ParticipantScores
);

public record MatchParticipantScoreRequest(
    Guid ParticipantId,
    int TotalScore
);
