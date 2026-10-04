namespace Boardy.Domain.Entities;

public class Result
{
    public int Id { get; set; }
    public int Score { get; set; }
    public int CriterionId { get; set; }
    public int ParticipantId { get; set; }
}