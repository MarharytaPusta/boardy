using Boardy.Domain.Enums;
using Boardy.Domain.Entities;
using Boardy.Application.GameCatalog.ScoreTemplate;
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
    private readonly IScoreTemplateRepository _scoreTemplateRepository;

    public SaveMatchResultsUseCase(
        IMatchRepository matchRepository,
        IScoreTemplateRepository scoreTemplateRepository,
        ILogger<SaveMatchResultsUseCase> logger)
    {
        _matchRepository = matchRepository;
        _scoreTemplateRepository = scoreTemplateRepository;
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

        var matchValidationResult = ValidateMatch(match, request);
        if (!matchValidationResult.IsSuccess)
        {
            return matchValidationResult;
        }

        var scoreTemplate = await _scoreTemplateRepository.GetByIdAsync(match.ScoreTemplateId);
        if (scoreTemplate is null)
        {
            return Result.Failure("Шаблон підрахунку балів не знайдено.");
        }

        var criteriaById = scoreTemplate.Criteria.ToDictionary(criterion => criterion.Id);
        var criterionValidationResult = ValidateCriterionScores(request, criteriaById);
        if (!criterionValidationResult.IsSuccess)
        {
            return criterionValidationResult;
        }

        foreach (var participantScore in request.ParticipantScores)
        {
            var totalScore = participantScore.CriterionScores.Sum(score =>
                criteriaById[score.CriterionId].IsNegative ? -score.Score : score.Score);

            foreach (var criterionScore in participantScore.CriterionScores)
            {
                match.Participants.First(participant => participant.Id == participantScore.ParticipantId)
                    .UpdateCriterionScore(criterionScore.CriterionId, criterionScore.Score);
            }

            match.UpdateParticipantScore(participantScore.ParticipantId, totalScore);
        }

        var highestScore = match.Participants.Max(participant => participant.TotalScore);
        var winnerIds = request.ParticipantScores
            .Where(score => match.Participants.First(participant => participant.Id == score.ParticipantId).TotalScore == highestScore)
            .Select(score => score.ParticipantId)
            .ToList();

        match.SetWinners(winnerIds);
        match.ChangeStatus(MatchStatus.Completed);

        await _matchRepository.UpdateAsync(match);

        _logger.LogInformation("Successfully saved results for match {MatchId}.", match.Id);

        return Result.Success();
    }

    private Result ValidateMatch(Match match, SaveMatchResultsRequest request)
    {
        if (match.Status == MatchStatus.Completed)
        {
            _logger.LogWarning("Match {MatchId} is already completed.", request.MatchId);
            return Result.Failure("Результати цієї партії вже збережено.");
        }

        var matchParticipantIds = match.Participants.Select(participant => participant.Id).ToHashSet();
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

        return Result.Success();
    }

    private Result ValidateCriterionScores(
        SaveMatchResultsRequest request,
        IReadOnlyDictionary<Guid, ScoreCriterion> criteriaById)
    {
        var containsInvalidCriterionScores = request.ParticipantScores.Any(participantScore =>
            participantScore.CriterionScores.Count != criteriaById.Count ||
            participantScore.CriterionScores.Select(score => score.CriterionId).Distinct().Count() != participantScore.CriterionScores.Count ||
            participantScore.CriterionScores.Any(score => !criteriaById.ContainsKey(score.CriterionId)));

        if (containsInvalidCriterionScores)
        {
            _logger.LogWarning("Validation failed: Criterion scores are incomplete or invalid for match {MatchId}.", request.MatchId);
            return Result.Failure("Для кожного учасника потрібно вказати бали за всіма критеріями.");
        }

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
