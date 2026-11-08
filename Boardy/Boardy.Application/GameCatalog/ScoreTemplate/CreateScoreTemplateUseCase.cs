using Boardy.Application.GameCatalog.Game;
using Boardy.Application.GameCatalog.ScoreTemplate;
using Boardy.Domain.Shared;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Boardy.Application.GameCatalog.ScoreTemplate;

public class CreateScoreTemplateUseCase
{
    private readonly IGameRepository _gameRepository;
    private readonly ILogger<CreateScoreTemplateUseCase> _logger;

    public CreateScoreTemplateUseCase(
        IGameRepository gameRepository,
        ILogger<CreateScoreTemplateUseCase> logger)
    {
        _gameRepository = gameRepository;
        _logger = logger;
    }

    public async Task<Result> ExecuteAsync(CreateScoreTemplateRequest request)
    {
        var validationResult = Validate(request);
        if (!validationResult.IsSuccess)
        {
            return validationResult;
        }

        var game = await _gameRepository.GetByIdAsync(request.GameId);
        if (game == null)
        {
            return Result.Failure("Гру не знайдено в базі даних.");
        }

        var template = new Domain.Entities.ScoreTemplate(request.TemplateName.Trim());

        foreach (var criterionDto in request.Criteria)
        {
            var criterion = new Domain.Entities.ScoreCriterion(criterionDto.Name.Trim(), criterionDto.IsNegative);
            template.AddCriterion(criterion);
        }

        game.AddScoreTemplate(template);
        await _gameRepository.UpdateAsync(game);

        _logger.LogInformation("Added ScoreTemplate '{TemplateName}' to Game ID {GameId}", template.Name, game.Id);

        return Result.Success();
    }

    private Result Validate(CreateScoreTemplateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.TemplateName))
            {
                _logger.LogWarning("Invalid input: Attempted to create a score template without a name");
                return Result.Failure("Назва шаблону не може бути порожньою");
            }

            if (request.Criteria == null || request.Criteria.Count == 0)
            {
                _logger.LogWarning("Invalid input: Attempted to create a score template '{TemplateName}' without any criteria", request.TemplateName);
                return Result.Failure("Додайте хоча б один критерій оцінювання");
            }

            if (request.Criteria.Any(c => string.IsNullOrWhiteSpace(c.Name)))
            {
                _logger.LogWarning("Invalid input: Attempted to create a score template '{TemplateName}' with empty criterion fields", request.TemplateName);
                return Result.Failure("Критерій не може бути порожнім");
            }

            bool hasDuplicates = request.Criteria
                .GroupBy(c => c.Name.Trim(), StringComparer.OrdinalIgnoreCase)
                .Any(g => g.Count() > 1);

            if (hasDuplicates)
            {
                return Result.Failure("Критерії в одному шаблоні не можуть мати однакові назви");
            }

            return Result.Success();
        }
    }