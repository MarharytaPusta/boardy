using Boardy.Domain.Enums;

namespace Boardy.Domain.Entities;

public class Match
{
    public Guid Id { get; private set; }

    public Guid ScoreTemplateId { get; private set; }

    public DateTime Date { get; private set; }

    public MatchStatus Status { get; private set; }

    private readonly List<MatchParticipant> _participants = new();
    public IReadOnlyCollection<MatchParticipant> Participants => _participants.AsReadOnly();

    private Match() { }

    public Match(Guid scoreTemplateId)
    {
        Id = Guid.NewGuid();
        ScoreTemplateId = scoreTemplateId;
        Date = DateTime.UtcNow; 
        Status = MatchStatus.InProgress;
    }

    public void AddParticipant(MatchParticipant participant)
    {
        _participants.Add(participant);
    }

    public void UpdateParticipantScore(Guid participantId, int totalScore)
    {
        var participant = _participants.First(p => p.Id == participantId);
        participant.UpdateTotalScore(totalScore);
    }

    public void SetWinners(IEnumerable<Guid> participantIds)
    {
        var winnerIds = participantIds.ToHashSet();

        foreach (var participant in _participants)
        {
            if (winnerIds.Contains(participant.Id))
            {
                participant.SetAsWinner();
            }
            else
            {
                participant.ResetWinner();
            }
        }
    }

    public void ChangeStatus(MatchStatus newStatus)
    {
        Status = newStatus;
    }
}
