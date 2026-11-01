namespace Boardy.Domain.Entities;

public class MatchCriterionResult
{
    public Guid Id { get; private set; }
    public Guid MatchParticipantId { get; }
    public Guid ScoreCriterionId { get; private set; }
    public int Score { get; private set; }

    private MatchCriterionResult() { }

    public MatchCriterionResult(Guid matchParticipantId, Guid scoreCriterionId, int score)
    {
        Id = Guid.NewGuid();
        MatchParticipantId = matchParticipantId;
        ScoreCriterionId = scoreCriterionId;
        Score = score;
    }

    public void UpdateScore(int score) => Score = score;
}
