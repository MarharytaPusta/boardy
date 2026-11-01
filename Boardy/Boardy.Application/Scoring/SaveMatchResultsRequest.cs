using System;
using System.Collections.Generic;

namespace Boardy.Application.Scoring;

public record SaveMatchResultsRequest(
    Guid MatchId,
    List<MatchParticipantScoreRequest> ParticipantScores
);

public record MatchParticipantScoreRequest(
    Guid ParticipantId,
    List<CriterionScoreRequest> CriterionScores
);

public record CriterionScoreRequest(Guid CriterionId, int Score);