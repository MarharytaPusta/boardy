using Boardy.Domain.Shared;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boardy.Application.Player;

public class DeletePlayerUseCase
{
    private readonly IPlayerRepository _playerRepository;
    private readonly ILogger<DeletePlayerUseCase> _logger;

    public DeletePlayerUseCase(IPlayerRepository playerRepository, ILogger<DeletePlayerUseCase> logger)
    {
        _playerRepository = playerRepository;
        _logger = logger;
    }

    public async Task<Result> ExecuteAsync(DeletePlayerRequest request)
    {
        _logger.LogInformation("Attempting to delete a player with id: {PlayerId}", request.PlayerId);

        var player = await _playerRepository.GetByIdAsync(request.PlayerId);

        if (player is null)
        {
            return Result.Failure("Гравця не знайдено.");
        }

        await _playerRepository.UpdateAsync(player);

        _logger.LogInformation("Player with ID {PlayerId} was deleted", player.Id);
        return Result.Success();
    }
}
