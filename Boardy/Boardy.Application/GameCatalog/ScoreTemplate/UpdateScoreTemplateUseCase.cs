using Boardy.Application.Authentication.Admin;
using Boardy.Application.GameCatalog.Game;
using Boardy.Domain.Entities;
using Boardy.Domain.Shared;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Boardy.Application.GameCatalog.ScoreTemplate;

public record UpdateScoreTemplateRequest(
    Guid GameId,
    Guid TemplateId,
    string TemplateName,
    List<CriterionDto> Criteria
);

public class UpdateScoreTemplateUseCase
{
    private readonly IGameRepository _gameRepository;
    private readonly IUserSession _userSession;
    private readonly ILogger<UpdateScoreTemplateUseCase> _logger;

    public UpdateScoreTemplateUseCase(
        IGameRepository gameRepository,
        IUserSession userSession,
        ILogger<UpdateScoreTemplateUseCase> logger)
    {
        _gameRepository = gameRepository;
        _userSession = userSession;
        _logger = logger;
    }

    public async Task<Result> ExecuteAsync(UpdateScoreTemplateRequest request)
    {
        var validationResult = Validate(request);
        if (!validationResult.IsSuccess)
        {
            return validationResult;
        }

        var game = await _gameRepository.GetByIdAsync(request.GameId);
        if (game is null)
        {
            _logger.LogWarning("Attempted to update template in non-existent game {GameId}.", request.GameId);
            return Result.Failure("Гру не знайдено.");
        }

        var template = game.ScoreTemplates.FirstOrDefault(t => t.Id == request.TemplateId);
        if (template is null)
        {
            _logger.LogWarning("ScoreTemplate {TemplateId} not found in Game {GameId}.", request.TemplateId, request.GameId);
            return Result.Failure("Шаблон оцінювання не знайдено.");
        }

        template.UpdateName(request.TemplateName.Trim());
        template.ClearCriteria();

        foreach (var criterionDto in request.Criteria)
        {
            var criterion = new Domain.Entities.ScoreCriterion(criterionDto.Name.Trim(), criterionDto.IsNegative);
            template.AddCriterion(criterion);
        }

        await _gameRepository.UpdateAsync(game);

        _logger.LogInformation(
            "Successfully updated ScoreTemplate '{TemplateName}' (ID: {TemplateId}) in Game ID {GameId}.",
            template.Name,
            template.Id,
            game.Id);

        return Result.Success();
    }

    private Result Validate(UpdateScoreTemplateRequest request)
    {
        if (!_userSession.IsAdminMode)
        {
            _logger.LogWarning(
                "Unauthorized attempt to update score template {TemplateId} in game {GameId}. User is not in admin mode.",
                request.TemplateId,
                request.GameId);
            return Result.Failure("Редагувати шаблони оцінювання може лише адміністратор.");
        }

        if (request.GameId == Guid.Empty)
        {
            _logger.LogWarning("Validation failed: GameId is empty.");
            return Result.Failure("Некоректний ідентифікатор гри.");
        }

        if (request.TemplateId == Guid.Empty)
        {
            _logger.LogWarning("Validation failed: TemplateId is empty.");
            return Result.Failure("Некоректний ідентифікатор шаблону оцінювання.");
        }

        if (string.IsNullOrWhiteSpace(request.TemplateName))
        {
            _logger.LogWarning("Validation failed: template name is empty.");
            return Result.Failure("Назва шаблону не може бути порожньою.");
        }

        if (request.Criteria == null || request.Criteria.Count == 0)
        {
            _logger.LogWarning("Validation failed: template without criteria.");
            return Result.Failure("Додайте хоча б один критерій оцінювання.");
        }

        if (request.Criteria.Any(c => string.IsNullOrWhiteSpace(c.Name)))
        {
            _logger.LogWarning("Validation failed: all criteria must have a name.");
            return Result.Failure("Усі критерії повинні мати назву. Заповніть порожні поля.");
        }

        bool hasDuplicates = request.Criteria
            .GroupBy(c => c.Name.Trim(), StringComparer.OrdinalIgnoreCase)
            .Any(g => g.Count() > 1);

        if (hasDuplicates)
        {
            _logger.LogWarning("Validation failed: the names of the criteria cannot be the same.");
            return Result.Failure("Критерії в одному шаблоні не можуть мати однакові назви.");
        }

        return Result.Success();
    }
}