namespace Boardy.Application.Scoring;

public record UpdateMatchResultsRequest(
    Guid MatchId,
    List<MatchParticipantScoreRequest> ParticipantScores
);
