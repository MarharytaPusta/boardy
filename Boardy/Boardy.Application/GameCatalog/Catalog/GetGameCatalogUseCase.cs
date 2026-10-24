using Boardy.Domain.Shared;
using Microsoft.Extensions.Logging;

namespace Boardy.Application.GameCatalog.Catalog;

public class GetGameCatalogUseCase
{
    private readonly IGameCatalogRepository _gameCatalogRepository;
    private readonly ILogger<GetGameCatalogUseCase> _logger;

    public GetGameCatalogUseCase(
        IGameCatalogRepository gameCatalogRepository,
        ILogger<GetGameCatalogUseCase> logger)
    {
        _gameCatalogRepository = gameCatalogRepository;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<GameCatalogItem>>> ExecuteAsync(GetGameCatalogRequest request)
    {
        var validationResult = Validate(request);
        if (!validationResult.IsSuccess)
        {
            _logger.LogWarning(
                "Game catalog request validation failed: {ErrorMessage}",
                validationResult.ErrorMessage);
            return Result<IEnumerable<GameCatalogItem>>.Failure(validationResult.ErrorMessage!);
        }

        var games = await _gameCatalogRepository.GetAllAsync();
        var filteredGames = games.Where(game =>
            (string.IsNullOrWhiteSpace(request.SearchQuery) ||
             game.Name.Contains(request.SearchQuery, StringComparison.OrdinalIgnoreCase)) &&
            (!request.PlayersCount.HasValue ||
             (game.MinPlayers <= request.PlayersCount.Value && game.MaxPlayers >= request.PlayersCount.Value)))
            .Select(game => new GameCatalogItem(
                game.Id,
                game.Name,
                game.Description,
                game.MinPlayers,
                game.MaxPlayers,
                game.Status))
            .ToList();

        _logger.LogInformation("Loaded game catalog");
        return Result<IEnumerable<GameCatalogItem>>.Success(filteredGames);
    }

    private static Result Validate(GetGameCatalogRequest request)
    {
        if (request.PlayersCount is <= 0)
        {
            return Result.Failure("Кількість гравців має бути більшою за 0.");
        }

        return Result.Success();
    }
}
