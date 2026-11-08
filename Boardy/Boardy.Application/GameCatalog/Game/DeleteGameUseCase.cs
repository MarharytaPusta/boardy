using Boardy.Domain.Shared;
using Microsoft.Extensions.Logging;

namespace Boardy.Application.GameCatalog.Game;

public class DeleteGameUseCase
{
    private readonly IGameRepository _gameRepository;
    private readonly ILogger<DeleteGameUseCase> _logger;

    public DeleteGameUseCase(
        IGameRepository gameRepository,
        ILogger<DeleteGameUseCase> logger)
    {
        _gameRepository = gameRepository;
        _logger = logger;
    }

    public async Task<Result> ExecuteAsync(DeleteGameRequest request)
    {
        _logger.LogInformation("Attempting to delete game with ID {GameId}", request.GameId);

        var game = await _gameRepository.GetByIdAsync(request.GameId);

        if (game is null)
        {
            _logger.LogWarning("Game with ID {GameId} was not found for deletion", request.GameId);
            return Result.Failure("Гру не знайдено.");
        }

        await _gameRepository.DeleteAsync(game);

        _logger.LogInformation("Game with ID {GameId} was deleted", game.Id);
        return Result.Success();
    }
}
