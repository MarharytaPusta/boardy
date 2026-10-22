using Boardy.Application.GameCatalog.ScoreTemplate;
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

    public async Task<Result> ExecuteAsync(CreateGameRequest request)
    {
        _logger.LogInformation("Attempting to create a new game: {GameName}", request.Name);

        var validationResult = Validate(request);
        if (!validationResult.IsSuccess)
        {
            return validationResult;
        }

        var newGame = new Domain.Entities.Game(
            request.Name,
            request.Description,
            request.MinPlayers,
            request.MaxPlayers,
            request.Status
        );

        try
        {
            await _gameRepository.AddAsync(newGame);
            _logger.LogInformation("Successfully created game with ID {GameId}", newGame.Id);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while saving the game {GameName}", request.Name);
            return Result.Failure("Сталася помилка при збереженні гри.");
        }
    }

    static private Result Validate(CreateGameRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result.Failure("Назва гри не може бути порожньою.");
        }

        if (request.MinPlayers < 1)
        {
            return Result.Failure("Мінімальна кількість гравців має бути більше 0.");
        }

        if (request.MinPlayers > request.MaxPlayers)
        {
            return Result.Failure("Мінімальна кількість гравців не може перевищувати максимальну.");
        }

        return Result.Success();
    }

}
