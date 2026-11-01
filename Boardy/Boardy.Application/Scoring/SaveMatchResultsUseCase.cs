using Boardy.Domain.Enums;
using Boardy.Domain.Shared;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Boardy.Application.Scoring;

public class SaveMatchResultsUseCase
{
    private readonly IMatchRepository _matchRepository;
    private readonly ILogger<SaveMatchResultsUseCase> _logger;

    public SaveMatchResultsUseCase(
        IMatchRepository matchRepository,
        ILogger<SaveMatchResultsUseCase> logger)
    {
        _matchRepository = matchRepository;
        _logger = logger;
    }

    public async Task<Result> ExecuteAsync(SaveMatchResultsRequest request)
    {
        var validationResult = Validate(request);
        if (!validationResult.IsSuccess)
        {
            return validationResult;
        }

        var match = await _matchRepository.GetByIdAsync(request.MatchId);
        if (match == null)
        {
            _logger.LogWarning("Match with ID {MatchId} not found.", request.MatchId);
            return Result.Failure("Партію не знайдено в базі даних.");
        }

        if (match.Status == MatchStatus.Completed)
        {
            _logger.LogWarning("Match {MatchId} is already completed.", request.MatchId);
            return Result.Failure("Результати цієї партії вже збережено.");
        }

        var matchParticipantIds = match.Participants.Select(participant => participant.Id).ToList();

        if (request.ParticipantScores.Any(score => !matchParticipantIds.Contains(score.ParticipantId)))
        {
            _logger.LogWarning("Validation failed: A participant does not belong to match {MatchId}.", request.MatchId);
            return Result.Failure("Учасника не знайдено в цій партії.");
        }

        if (request.ParticipantScores.Count != match.Participants.Count)
        {
            _logger.LogWarning("Validation failed: Not all participants have results for match {MatchId}.", request.MatchId);
            return Result.Failure("Не для всіх учасників вказано результати.");
        }

        foreach (var score in request.ParticipantScores)
        {
            match.UpdateParticipantScore(score.ParticipantId, score.TotalScore);
        }

        var highestScore = request.ParticipantScores.Max(score => score.TotalScore);
        var winnerIds = request.ParticipantScores
            .Where(score => score.TotalScore == highestScore)
            .Select(score => score.ParticipantId)
            .ToList();

        match.SetWinners(winnerIds);
        match.ChangeStatus(MatchStatus.Completed);

        await _matchRepository.UpdateAsync(match);

        _logger.LogInformation("Successfully saved results for match {MatchId}.", match.Id);

        return Result.Success();
    }

    private Result Validate(SaveMatchResultsRequest request)
    {
        if (request.MatchId == Guid.Empty)
        {
            _logger.LogWarning("Validation failed: MatchId is empty.");
            return Result.Failure("Не вказано ідентифікатор партії.");
        }

        if (request.ParticipantScores == null || !request.ParticipantScores.Any())
        {
            _logger.LogWarning("Validation failed: No participant scores provided.");
            return Result.Failure("Не вказано результати учасників.");
        }

        if (request.ParticipantScores.Select(score => score.ParticipantId).Distinct().Count() != request.ParticipantScores.Count)
        {
            _logger.LogWarning("Validation failed: Duplicate participant scores found.");
            return Result.Failure("Результати одного учасника не можуть бути вказані двічі.");
        }

        return Result.Success();
    }
}
