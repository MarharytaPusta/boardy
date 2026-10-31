using Boardy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        Status = MatchStatus.Created;
    }

    public void AddParticipant(MatchParticipant participant)
    {
        _participants.Add(participant);
    }

    public void ChangeStatus(MatchStatus newStatus)
    {
        Status = newStatus;
    }
}
