using Boardy.Application.GameCatalog.Game;
using Boardy.Application.GameCatalog.ScoreTemplate;
using Boardy.Application.Player;
using Boardy.Domain.Entities;
using Boardy.Domain.Enums;
using Boardy.Domain.Shared;
using Microsoft.Extensions.Logging;

namespace Boardy.Application.Scoring;

public class UpdateMatchResultsUseCase
{
    private readonly IMatchRepository _matchRepository;
    private readonly IScoreTemplateRepository _scoreTemplateRepository;
    private readonly IPlayerRepository _playerRepository;
    private readonly IGameRepository _gameRepository;
    private readonly ILogger<UpdateMatchResultsUseCase> _logger;

    public UpdateMatchResultsUseCase(
        IMatchRepository matchRepository,
        IScoreTemplateRepository scoreTemplateRepository,
        IPlayerRepository playerRepository,
        IGameRepository gameRepository,
        ILogger<UpdateMatchResultsUseCase> logger)
    {
        _matchRepository = matchRepository;
        _scoreTemplateRepository = scoreTemplateRepository;
        _playerRepository = playerRepository;
        _gameRepository = gameRepository;
        _logger = logger;
    }

    public async Task<Result> ExecuteAsync(UpdateMatchResultsRequest request)
    {
        var validationResult = Validate(request);
        if (!validationResult.IsSuccess)
        {
            return validationResult;
        }

        var match = await _matchRepository.GetByIdAsync(request.MatchId);
        if (match is null)
        {
            _logger.LogWarning("Match with ID {MatchId} was not found.", request.MatchId);
            return Result.Failure("Партію не знайдено в базі даних.");
        }

        if (match.Status != MatchStatus.Completed)
        {
            _logger.LogWarning("Match {MatchId} is not completed and cannot be edited.", request.MatchId);
            return Result.Failure("Редагувати можна лише завершену партію.");
        }

        var matchValidationResult = ValidateMatchParticipants(match, request);
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

        ApplyResults(match, request, criteriaById);
        await _matchRepository.UpdateAsync(match);

        var statisticsResult = await RecalculateParticipantStatisticsAsync(match);
        if (!statisticsResult.IsSuccess)
        {
            return statisticsResult;
        }

        _logger.LogInformation("Successfully updated results for match {MatchId}.", match.Id);
        return Result.Success();
    }

    private static void ApplyResults(
        Match match,
        UpdateMatchResultsRequest request,
        IReadOnlyDictionary<Guid, ScoreCriterion> criteriaById)
    {
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
        var winnerIds = match.Participants
            .Where(participant => participant.TotalScore == highestScore)
            .Select(participant => participant.Id);

        match.SetWinners(winnerIds);
    }

    private async Task<Result> RecalculateParticipantStatisticsAsync(Match match)
    {
        var playerIds = match.Participants
            .Select(participant => participant.PlayerId)
            .Distinct()
            .ToList();
        var players = (await _playerRepository.GetByIdsAsync(playerIds)).ToList();
        if (players.Count != playerIds.Count)
        {
            _logger.LogWarning("Statistics recalculation failed because one or more match participants were not found.");
            return Result.Failure("Одного або кількох гравців не знайдено.");
        }

        var completedMatches = (await _matchRepository.GetCompletedByPlayerIdsAsync(playerIds)).ToList();
        var scoreTemplateIds = completedMatches
            .Select(completedMatch => completedMatch.ScoreTemplateId)
            .Distinct()
            .ToList();
        var gameIdsByScoreTemplateId = await _gameRepository.GetGameIdsByScoreTemplateIdsAsync(scoreTemplateIds);

        var matchesWithoutGame = completedMatches
            .Where(completedMatch => !gameIdsByScoreTemplateId.ContainsKey(completedMatch.ScoreTemplateId))
            .ToList();
        if (matchesWithoutGame.Count > 0)
        {
            _logger.LogWarning(
                "Skipped {MatchCount} completed matches without an associated game during statistics recalculation.",
                matchesWithoutGame.Count);
        }

        foreach (var player in players)
        {
            var playerMatches = completedMatches
                .Where(completedMatch => completedMatch.Participants.Any(
                    participant => participant.PlayerId == player.Id))
                .ToList();
            var playedGamesCount = playerMatches
                .Where(completedMatch => gameIdsByScoreTemplateId.ContainsKey(completedMatch.ScoreTemplateId))
                .Select(completedMatch => gameIdsByScoreTemplateId[completedMatch.ScoreTemplateId])
                .Distinct()
                .Count();
            var winsCount = playerMatches.Count(completedMatch => completedMatch.Participants.Any(
                participant => participant.PlayerId == player.Id && participant.IsWinner));

            player.UpdateStatistics(playedGamesCount, playerMatches.Count, winsCount);
            await _playerRepository.UpdateAsync(player);
        }

        return Result.Success();
    }

    private Result ValidateMatchParticipants(Match match, UpdateMatchResultsRequest request)
    {
        var matchParticipantIds = match.Participants.Select(participant => participant.Id).ToHashSet();
        if (request.ParticipantScores.Any(score => !matchParticipantIds.Contains(score.ParticipantId)))
        {
            _logger.LogWarning("A participant does not belong to match {MatchId}.", request.MatchId);
            return Result.Failure("Учасника не знайдено в цій партії.");
        }

        if (request.ParticipantScores.Count != match.Participants.Count)
        {
            _logger.LogWarning("Not all participants have results for match {MatchId}.", request.MatchId);
            return Result.Failure("Не для всіх учасників вказано результати.");
        }

        return Result.Success();
    }

    private Result ValidateCriterionScores(
        UpdateMatchResultsRequest request,
        IReadOnlyDictionary<Guid, ScoreCriterion> criteriaById)
    {
        var containsInvalidCriterionScores = request.ParticipantScores.Any(participantScore =>
            participantScore.CriterionScores.Count != criteriaById.Count ||
            participantScore.CriterionScores.Select(score => score.CriterionId).Distinct().Count() != participantScore.CriterionScores.Count ||
            participantScore.CriterionScores.Any(score => !criteriaById.ContainsKey(score.CriterionId)));

        if (containsInvalidCriterionScores)
        {
            _logger.LogWarning("Criterion scores are incomplete or invalid for match {MatchId}.", request.MatchId);
            return Result.Failure("Для кожного учасника потрібно вказати бали за всіма критеріями.");
        }

        return Result.Success();
    }

    private Result Validate(UpdateMatchResultsRequest request)
    {
        if (request.MatchId == Guid.Empty)
        {
            _logger.LogWarning("Validation failed because MatchId is empty.");
            return Result.Failure("Не вказано ідентифікатор партії.");
        }

        if (request.ParticipantScores is null || request.ParticipantScores.Count == 0)
        {
            _logger.LogWarning("Validation failed because no participant results were provided.");
            return Result.Failure("Не вказано результати учасників.");
        }

        if (request.ParticipantScores.Select(score => score.ParticipantId).Distinct().Count() != request.ParticipantScores.Count)
        {
            _logger.LogWarning("Validation failed because duplicate participant results were provided.");
            return Result.Failure("Результати одного учасника не можуть бути вказані двічі.");
        }

        return Result.Success();
    }
}
