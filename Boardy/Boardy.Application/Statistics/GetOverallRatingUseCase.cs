using Boardy.Domain.Shared;
using Boardy.Application.Player;
using Microsoft.Extensions.Logging;

namespace Boardy.Application.Statistics;

public class GetOverallRatingUseCase
{
    private readonly IPlayerRepository _playerRepository;
    private readonly ILogger<GetOverallRatingUseCase> _logger;

    public GetOverallRatingUseCase(
        IPlayerRepository playerRepository,
        ILogger<GetOverallRatingUseCase> logger)
    {
        _playerRepository = playerRepository;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<GetOverallRatingDto>>> ExecuteAsync()
    {
        var players = await _playerRepository.GetAllAsync();
        var rating = players
            .OrderByDescending(player => player.WinsCount)
            .ThenByDescending(player => player.PlayedMatchesCount)
            .ThenByDescending(player => player.PlayedGamesCount)
            .ThenBy(player => player.Name)
            .Select(player => new GetOverallRatingDto(
                player.Id,
                player.Name,
                player.PlayedGamesCount,
                player.PlayedMatchesCount,
                player.WinsCount))
            .ToList();

        _logger.LogInformation("Loaded player rating with {PlayerCount} players.", rating.Count);
        return Result<IEnumerable<GetOverallRatingDto>>.Success(rating);
    }
}
