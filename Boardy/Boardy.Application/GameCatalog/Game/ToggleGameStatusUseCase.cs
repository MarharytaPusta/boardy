using Boardy.Application.GameCatalog.Catalog;
using Boardy.Domain.Enums;
using Boardy.Domain.Shared;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boardy.Application.GameCatalog.Game;

public class ToggleGameStatusUseCase
{
    private readonly IGameRepository _gameRepository;
    private readonly ILogger<ToggleGameStatusUseCase> _logger;

    public ToggleGameStatusUseCase(IGameRepository gameRepository, ILogger<ToggleGameStatusUseCase> logger)
    {
        _gameRepository = gameRepository;
        _logger = logger;
    }

    public async Task<Result<bool>> ExecuteAsync(ToggleGameStatusRequest request)
    {
        var game = await _gameRepository.GetByIdAsync(request.GameId);
        if (game == null)
        {
            _logger.LogWarning("Can't change game status: game not found");
            return Result<bool>.Failure("Гру не знайдено.");
        }

        var newStatus = game.Status == GameStatus.Active ? GameStatus.Inactive : GameStatus.Active;
        _logger.LogInformation("Changing game status from {1} to {2}", game.Status, newStatus);
        game.ChangeStatus(newStatus);

        await _gameRepository.UpdateAsync(game);
        _logger.LogInformation("Changed game status succesefully");
        return Result<bool>.Success(true);
    }
}
