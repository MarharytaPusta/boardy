using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boardy.Domain.Entities;

public class MatchParticipant
{
    public Guid Id { get; private set; }

    public Guid PlayerId { get; private set; }

    public Guid MatchId { get; }

    public int TotalScore { get; private set; }

    public bool IsWinner { get; private set; }

    private MatchParticipant() { }

    public MatchParticipant(Guid playerId)
    {
        Id = Guid.NewGuid();
        PlayerId = playerId;
        TotalScore = 0;
        IsWinner = false;
    }

    public void UpdateTotalScore(int score)
    {
        TotalScore = score;
    }

    public void SetAsWinner()
    {
        IsWinner = true;
    }

    public void ResetWinner()
    {
        IsWinner = false;
    }

}
