using Boardy.Application.GameCatalog.Game;
using Boardy.Application.GameCatalog.ScoreTemplate;
using Boardy.Domain.Shared;
using Microsoft.Extensions.Logging;
using System.Linq;
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

        public async Task<Result<GameDetailsDto>> ExecuteAsync(GetGameDetailsRequest request)
        {
            var game = await _gameRepository.GetByIdAsync(request.GameId);

            if (game == null)
            {
                _logger.LogWarning("Game with ID {GameId} not found.", request.GameId);
                return Result<GameDetailsDto>.Failure("Гру не знайдено в базі даних.");
            }

            var templatesDto = game.ScoreTemplates.Select(t => new ScoreTemplateDto(
                t.Name,
                t.Criteria.Select(c => new CriterionDto(c.Name, c.IsNegative)).ToList()
            )).ToList();

            var gameDto = new GameDetailsDto(
                game.Id,
                game.Name,
                game.Description,
                game.MinPlayers,
                game.MaxPlayers,
                game.Status,
                templatesDto
            );

            return Result<GameDetailsDto>.Success(gameDto);
        }
    }
}