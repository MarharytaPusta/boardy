namespace Boardy.Application.Scoring;

public record GetMatchHistoryDto(
    Guid Id,
    DateTime Date,
    string GameName,
    string Participants,
    string Winners);
