using Boardy.Domain.Shared;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Linq;

namespace Boardy.Application.GameCatalog.Game;

public class UpdateGameUseCase
{
    private readonly IGameRepository _gameRepository;
    private readonly ILogger<UpdateGameUseCase> _logger;

    public UpdateGameUseCase(IGameRepository gameRepository, ILogger<UpdateGameUseCase> logger)
    {
        _gameRepository = gameRepository;
        _logger = logger;
    }

    public async Task<Result> ExecuteAsync(UpdateGameRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            _logger.LogWarning("Validation failed: Game name is empty.");
            return Result.Failure("Назва гри не може бути порожньою.");
        }

        if (request.MinPlayers < 1 || request.MaxPlayers < request.MinPlayers)
        {
            _logger.LogWarning("Validation failed: invalid number of players");
            return Result.Failure("Некоректно вказана кількість гравців.");
        }

        var game = await _gameRepository.GetByIdAsync(request.GameId);

        if (game is null)
        {
            _logger.LogWarning("Game with ID {GameId} was not found for deletion", request.GameId);
            return Result.Failure("Гру не знайдено.");
        }

        game.UpdateDetails(
            request.Name.Trim(),
            request.Description?.Trim() ?? string.Empty,
            request.MinPlayers,
            request.MaxPlayers
        );

        await _gameRepository.UpdateAsync(game);

        _logger.LogInformation("Game with ID {GameId} was updated", game.Id);

        return Result.Success();
    }
}