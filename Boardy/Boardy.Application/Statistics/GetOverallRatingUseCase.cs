using Boardy.Domain.Shared;
using Boardy.Application.Player;
using Microsoft.Extensions.Logging;

namespace Boardy.Application.Statistics;

public class GetOverallRatingUseCase
{
    private readonly IPlayerStatisticsRepository _playerStatisticsRepository;
    private readonly ILogger<GetOverallRatingUseCase> _logger;

    public GetOverallRatingUseCase(
        IPlayerStatisticsRepository playerStatisticsRepository,
        ILogger<GetOverallRatingUseCase> logger)
    {
        _playerStatisticsRepository = playerStatisticsRepository;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<GetOverallRatingDto>>> ExecuteAsync(
            CancellationToken cancellationToken = default)
    {
        var rating = await _playerStatisticsRepository.GetOverallRatingAsync(cancellationToken);

        _logger.LogInformation("Loaded player rating with {PlayerCount} players.", rating.Count);
        return Result<IEnumerable<GetOverallRatingDto>>.Success(rating);
    }
}
