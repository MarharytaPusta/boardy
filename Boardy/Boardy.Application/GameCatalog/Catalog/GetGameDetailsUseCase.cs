using Boardy.Application.GameCatalog.Game;
using Boardy.Domain.Shared;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Boardy.Application.GameCatalog.Catalog
{
    public class GetGameDetailsUseCase
    {
        private readonly IGameRepository _gameRepository;
        private readonly ILogger<GetGameDetailsUseCase> _logger;

        public GetGameDetailsUseCase(
            IGameRepository gameRepository,
            ILogger<GetGameDetailsUseCase> logger)
        {
            _gameRepository = gameRepository;
            _logger = logger;
        }

        public async Task<Result<Domain.Entities.Game>> ExecuteAsync(GetGameDetailsRequest request)
        {
            var game = await _gameRepository.GetByIdAsync(request.GameId);

            if (game == null)
            {
                _logger.LogWarning("Game with ID {GameId} not found.", request.GameId);
                return Result<Domain.Entities.Game>.Failure("Гру не знайдено в базі даних.");
            }

            _logger.LogInformation("Successfully loaded details for game '{GameName}' (ID: {GameId}).", game.Name, game.Id);
            return Result<Domain.Entities.Game>.Success(game);
        }
    }
}