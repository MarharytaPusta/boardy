using Boardy.Domain.Shared;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Boardy.Application.Player
{
    public class UpdatePlayerUseCase
    {
        private readonly IPlayerRepository _playerRepository;
        private readonly ILogger<UpdatePlayerUseCase> _logger;

        public UpdatePlayerUseCase(IPlayerRepository playerRepository, ILogger<UpdatePlayerUseCase> logger)
        {
            _playerRepository = playerRepository;
            _logger = logger;
        }

        public async Task<Result> ExecuteAsync(UpdatePlayerRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return Result.Failure("Ім'я не може бути порожнім.");
            }

            var player = await _playerRepository.GetByIdAsync(request.PlayerId);

            if (player is null)
            {
                return Result.Failure("Гравця не знайдено.");
            }

            player.Name = request.Name.Trim();
            await _playerRepository.UpdateAsync(player);

            _logger.LogInformation("Player with ID {PlayerId} was updated", player.Id);
            return Result.Success();
        }
    }
}
