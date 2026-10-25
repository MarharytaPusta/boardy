using Boardy.Application.GameCatalog.Game;
using Boardy.Domain.Shared;
using Microsoft.Extensions.Logging;

namespace Boardy.Application.GameCatalog.Catalog;

public class GetGameCatalogUseCase
{
    private readonly IGameRepository _gameRepository;
    private readonly ILogger<GetGameCatalogUseCase> _logger;

    public GetGameCatalogUseCase(
        IGameRepository gameRepository,
        ILogger<GetGameCatalogUseCase> logger)
    {
        _gameRepository = gameRepository;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<Domain.Entities.Game>>> ExecuteAsync(GetGameCatalogRequest request)
    {
        var validationResult = Validate(request);
        if (!validationResult.IsSuccess)
        {
            return Result<IEnumerable<Domain.Entities.Game>>.Failure(validationResult.ErrorMessage!);
        }

        var games = await _gameRepository.GetAllAsync();
        var filteredGames = games.Where(game =>
            (string.IsNullOrWhiteSpace(request.SearchQuery) ||
             game.Name.Contains(request.SearchQuery, StringComparison.OrdinalIgnoreCase)) &&
            (!request.PlayersCount.HasValue ||
             (game.MinPlayers <= request.PlayersCount.Value && game.MaxPlayers >= request.PlayersCount.Value)))
            .ToList();

        _logger.LogInformation("Loaded game catalog");
        return Result<IEnumerable<Domain.Entities.Game>>.Success(filteredGames);
    }

    private Result Validate(GetGameCatalogRequest request)
    {
        if (request.PlayersCount is <= 0)
        {
            _logger.LogWarning(
                "Validation failed: PlayersCount is less than 1 (Provided: {PlayersCount}).",
                request.PlayersCount);
            return Result.Failure("Кількість гравців має бути більшою за 0.");
        }

        return Result.Success();
    }
}
