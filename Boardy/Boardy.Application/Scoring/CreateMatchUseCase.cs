using Boardy.Domain.Entities;
using Boardy.Domain.Shared;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boardy.Application.Scoring;

public class CreateMatchUseCase
{
    private readonly IMatchRepository _matchRepository;
    private readonly ILogger<CreateMatchUseCase> _logger;

    public CreateMatchUseCase(IMatchRepository matchRepository, ILogger<CreateMatchUseCase> logger)
    {
        _matchRepository = matchRepository;
        _logger = logger;
    }
    public async Task<Result<Guid>> ExecuteAsync(CreateMatchRequest request)
    {
        _logger.LogInformation("Attempting to create a new match for template {TemplateId}", request.ScoreTemplateId);

        var validationResult = Validate(request);
        if (!validationResult.IsSuccess)
        {
            return Result<Guid>.Failure(validationResult.ErrorMessage!);
        }

        var match = new Match(request.ScoreTemplateId);

        foreach (var playerId in request.PlayerIds)
        {
            var participant = new MatchParticipant(playerId);
            match.AddParticipant(participant);
        }

        await _matchRepository.AddAsync(match);

        _logger.LogInformation("Successfully created match {MatchId} with {ParticipantCount} participants",
            match.Id, match.Participants.Count);

        return Result<Guid>.Success(match.Id);
    }

    private Result Validate(CreateMatchRequest request)
    {
        if (request.ScoreTemplateId == Guid.Empty)
        {
            _logger.LogWarning("Validation failed: ScoreTemplateId is empty.");
            return Result.Failure("Шаблон гри не знайдено.");
        }

        if (request.PlayerIds == null || !request.PlayerIds.Any())
        {
            _logger.LogWarning("Validation failed: No players selected.");
            return Result.Failure("Для початку партії необхідно обрати хоча б одного гравця.");
        }

        if (request.PlayerIds.Distinct().Count() != request.PlayerIds.Count)
        {
            _logger.LogWarning("Validation failed: Duplicate players found in the request.");
            return Result.Failure("Один і той самий гравець не може бути доданий до партії двічі.");
        }

        return Result.Success();
    }
}
