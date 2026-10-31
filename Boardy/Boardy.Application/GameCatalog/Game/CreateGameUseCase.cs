using Boardy.Application.GameCatalog.ScoreTemplate;
using Boardy.Domain.Entities;
using Boardy.Domain.Shared;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boardy.Application.GameCatalog.Game;

public class CreateGameUseCase
{
    private readonly IGameRepository _gameRepository;
    private readonly ILogger<CreateGameUseCase> _logger;

    public CreateGameUseCase(IGameRepository gameRepository, ILogger<CreateGameUseCase> logger)
    {
        _gameRepository = gameRepository;
        _logger = logger;
    }

    public async Task<Result<Guid>> ExecuteAsync(CreateGameRequest request)
    {
        _logger.LogInformation("Attempting to create a new game: {GameName}", request.Name);

        var validationResult = Validate(request);
        if (!validationResult.IsSuccess)
        {
            return Result<Guid>.Failure(validationResult.ErrorMessage ?? "Невідома помилка валідації.");
        }

        var newGame = new Domain.Entities.Game(
            request.Name,
            request.Description,
            request.MinPlayers,
            request.MaxPlayers,
            request.Status
        );

        var template = new Domain.Entities.ScoreTemplate("Базовий шаблон");
        foreach (var critDto in request.Criteria)
        {
            template.AddCriterion(new Domain.Entities.ScoreCriterion(critDto.Name, critDto.IsNegative));
        }

        newGame.AddScoreTemplate(template);

        await _gameRepository.AddAsync(newGame);
        _logger.LogInformation("Successfully created game with ID {GameId}", newGame.Id);

        return Result<Guid>.Success(newGame.Id);
    }

    private Result Validate(CreateGameRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            _logger.LogWarning("Validation failed: Game name is empty or null.");
            return Result.Failure("Назва гри не може бути порожньою.");
        }

        if (request.MinPlayers < 1)
        {
            _logger.LogWarning("Validation failed: MinPlayers is less than 1 (Provided: {MinPlayers}).", request.MinPlayers);
            return Result.Failure("Мінімальна кількість гравців має бути більше 0.");
        }

        if (request.MinPlayers > request.MaxPlayers)
        {
            _logger.LogWarning("Validation failed: MinPlayers ({MinPlayers}) is greater than MaxPlayers ({MaxPlayers}).",
            request.MinPlayers, request.MaxPlayers);
            return Result.Failure("Мінімальна кількість гравців не може перевищувати максимальну.");
        }

        if (request.Criteria == null || request.Criteria.Count == 0)
        {
            return Result.Failure("Додайте хоча б один критерій оцінювання");
        }

        if (request.Criteria.Any(c => string.IsNullOrWhiteSpace(c.Name)))
        {
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
