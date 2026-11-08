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
            return Result.Failure("Назва гри не може бути порожньою.");
        }

        if (request.MinPlayers < 1 || request.MaxPlayers < request.MinPlayers)
        {
            return Result.Failure("Некоректно вказана кількість гравців.");
        }

        var game = await _gameRepository.GetByIdAsync(request.GameId);

        if (game is null)
        {
            return Result.Failure("Гру не знайдено.");
        }

        game.UpdateDetails(
            request.Name.Trim(),
            request.Description?.Trim() ?? string.Empty,
            request.MinPlayers,
            request.MaxPlayers,
            request.Status
        );

        await _gameRepository.UpdateAsync(game);

        await _gameRepository.UpdateAsync(game);

        _logger.LogInformation("Game with ID {GameId} was updated", game.Id);

        return Result.Success();
    }
}