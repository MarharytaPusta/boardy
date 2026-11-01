using Boardy.Application.Player;
using Boardy.Application.GameCatalog.ScoreTemplate;
using Boardy.Application.GameCatalog.Game;
using Boardy.Domain.Shared;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Boardy.Application.Scoring;

public class GetMatchResultsUseCase
{
    private readonly IMatchRepository _matchRepository;
    private readonly IPlayerRepository _playerRepository;
    private readonly IScoreTemplateRepository _scoreTemplateRepository;
    private readonly IGameRepository _gameRepository;
    private readonly ILogger<GetMatchResultsUseCase> _logger;

    public GetMatchResultsUseCase(
        IMatchRepository matchRepository,
        IPlayerRepository playerRepository,
        IScoreTemplateRepository scoreTemplateRepository,
        IGameRepository gameRepository,
        ILogger<GetMatchResultsUseCase> logger)
    {
        _matchRepository = matchRepository;
        _playerRepository = playerRepository;
        _scoreTemplateRepository = scoreTemplateRepository;
        _gameRepository = gameRepository;
        _logger = logger;
    }

    public async Task<Result<MatchResultsDto>> ExecuteAsync(GetMatchResultsRequest request)
    {
        var validationResult = Validate(request);
        if (!validationResult.IsSuccess)
        {
            return Result<MatchResultsDto>.Failure(validationResult.ErrorMessage!);
        }

        var match = await _matchRepository.GetByIdAsync(request.MatchId);

        if (match == null)
        {
            _logger.LogWarning("Match with ID {MatchId} not found.", request.MatchId);
            return Result<MatchResultsDto>.Failure("Партію не знайдено в базі даних.");
        }

        var scoreTemplate = await _scoreTemplateRepository.GetByIdAsync(match.ScoreTemplateId);
        if (scoreTemplate is null)
        {
            return Result<MatchResultsDto>.Failure("Шаблон підрахунку балів не знайдено.");
        }

        var criteria = scoreTemplate.Criteria
            .Select(criterion => new ScoreCriterionDto(criterion.Id, criterion.Name, criterion.IsNegative))
            .ToList();
        var gameName = await _gameRepository.GetNameByScoreTemplateIdAsync(match.ScoreTemplateId)
            ?? "Гра";

        var playerIds = match.Participants
            .Select(participant => participant.PlayerId)
            .Distinct()
            .ToList();

        var players = await _playerRepository.GetByIdsAsync(playerIds);
        var playerNames = players.ToDictionary(player => player.Id, player => player.Name);

        var participantsDto = match.Participants.Select(participant =>
        {
            var playerName = playerNames.TryGetValue(participant.PlayerId, out var name)
                ? name
                : "Невідомий гравець";

            return new MatchParticipantResultDto(
                participant.Id,
                participant.PlayerId,
                playerName,
                participant.TotalScore,
                participant.IsWinner,
                participant.CriterionResults.Select(criterionResult =>
                    new CriterionResultDto(criterionResult.ScoreCriterionId, criterionResult.Score)).ToList()
            );
        }).ToList();

        var matchResultsDto = new MatchResultsDto(
            match.Id,
            match.Date,
            match.Status,
            gameName,
            criteria,
            participantsDto
        );

        return Result<MatchResultsDto>.Success(matchResultsDto);
    }

    private Result Validate(GetMatchResultsRequest request)
    {
        if (request.MatchId == Guid.Empty)
        {
            _logger.LogWarning("Validation failed: MatchId is empty.");
            return Result.Failure("Не вказано ідентифікатор партії.");
        }

        return Result.Success();
    }
}
