using Boardy.Application.Player;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Boardy.Application.Player
{
    public class CreatePlayerUseCase
    {
        private readonly IPlayerRepository _playerRepository;
        private readonly ILogger<CreatePlayerUseCase> _logger;

        public CreatePlayerUseCase(IPlayerRepository playerRepository, ILogger<CreatePlayerUseCase> logger)
        {
            _playerRepository = playerRepository;
            _logger = logger;
        }

        public async Task ExecuteAsync(CreatePlayerRequest request)
        {
            _logger.LogInformation("Attempting to create a new player with name: {PlayerName}", request.Name);

            var player = new Domain.Entities.Player(request.Name);

            await _playerRepository.AddAsync(player);

            _logger.LogInformation("Successfully created and saved player with ID: {PlayerId}", player.Id);
        }
    }
}