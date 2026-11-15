using Boardy.Application.Authentication.Admin;
using Boardy.Domain.Entities;
using Boardy.Domain.Shared;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boardy.Application.Scoring;

public record DeleteMatchRequest(Guid MatchId);

public class DeleteMatchUseCase
{
    private readonly IMatchRepository _matchRepository;
    private readonly IUserSession _userSession;
    private readonly ILogger<DeleteMatchUseCase> _logger;

    public DeleteMatchUseCase(
        IMatchRepository matchRepository,
        IUserSession userSession,
        ILogger<DeleteMatchUseCase> logger)
    {
        _matchRepository = matchRepository;
        _userSession = userSession;
        _logger = logger;
    }

    public async Task<Result> ExecuteAsync(DeleteMatchRequest request)
    {
        _logger.LogInformation("Attempting to delete match with ID {MatchId}.", request.MatchId);

        var match = request.MatchId != Guid.Empty
            ? await _matchRepository.GetByIdAsync(request.MatchId)
            : null;

        var validationResult = Validate(request, match);
        if (!validationResult.IsSuccess)
        {
            return validationResult;
        }

        await _matchRepository.DeleteAsync(match!);

        _logger.LogInformation("Match with ID {MatchId} was successfully deleted.", request.MatchId);
        return Result.Success();
    }

    private Result Validate(DeleteMatchRequest request, Match? match)
    {
        if (!_userSession.IsAdminMode)
        {
            _logger.LogWarning(
                "Unauthorized attempt to delete match {MatchId}. User is not in admin mode.",
                request.MatchId);
            return Result.Failure("Видаляти партії може лише адміністратор.");
        }

        if (request.MatchId == Guid.Empty)
        {
            _logger.LogWarning("Match deletion validation failed: MatchId is empty.");
            return Result.Failure("Некоректний ідентифікатор партії.");
        }

        if (match is null)
        {
            _logger.LogWarning(
                "Match deletion validation failed: Match with ID {MatchId} was not found.",
                request.MatchId);
            return Result.Failure("Партію не знайдено.");
        }

        return Result.Success();
    }
}
