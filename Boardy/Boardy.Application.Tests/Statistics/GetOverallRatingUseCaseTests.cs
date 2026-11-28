using Boardy.Application.Player;
using Boardy.Application.Statistics;
using Microsoft.Extensions.Logging;
using Moq;

namespace Boardy.Application.Tests.Statistics;

public class GetOverallRatingUseCaseTests
{
    private readonly Mock<IPlayerStatisticsRepository> _playerStatisticsRepositoryMock = new();
    private readonly GetOverallRatingUseCase _useCase;

    public GetOverallRatingUseCaseTests()
    {
        var loggerMock = new Mock<ILogger<GetOverallRatingUseCase>>();
        _useCase = new GetOverallRatingUseCase(_playerStatisticsRepositoryMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsPlayerStatistics()
    {
        var playerId = Guid.NewGuid();
        var ratingDto = new GetOverallRatingDto(
            playerId,
            "Олена",
            PlayedGamesCount: 3,
            PlayedMatchesCount: 5,
            WinsCount: 2);

        _playerStatisticsRepositoryMock
            .Setup(repository => repository.GetOverallRatingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { ratingDto });

        var result = await _useCase.ExecuteAsync();

        Assert.True(result.IsSuccess);
        var ratingItem = Assert.Single(result.Data!);
        Assert.Equal(playerId, ratingItem.PlayerId);
        Assert.Equal("Олена", ratingItem.PlayerName);
        Assert.Equal(3, ratingItem.PlayedGamesCount);
        Assert.Equal(5, ratingItem.PlayedMatchesCount);
        Assert.Equal(2, ratingItem.WinsCount);
    }

    [Fact]
    public async Task ExecuteAsync_WithMultiplePlayers_PreservesRepositoryOrder()
    {
        var expectedRating = new[]
        {
            new GetOverallRatingDto(Guid.NewGuid(), "Богдан", 2, 4, 2),
            new GetOverallRatingDto(Guid.NewGuid(), "Анна", 1, 4, 2),
            new GetOverallRatingDto(Guid.NewGuid(), "Ганна", 1, 4, 2),
            new GetOverallRatingDto(Guid.NewGuid(), "Віра", 3, 5, 1)
        };

        _playerStatisticsRepositoryMock
            .Setup(repository => repository.GetOverallRatingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedRating);

        var result = await _useCase.ExecuteAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal(
            new[] { "Богдан", "Анна", "Ганна", "Віра" },
            result.Data!.Select(item => item.PlayerName));
    }

    [Fact]
    public async Task ExecuteAsync_WithNoPlayers_ReturnsEmptyRating()
    {
        _playerStatisticsRepositoryMock
            .Setup(repository => repository.GetOverallRatingAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<GetOverallRatingDto>());

        var result = await _useCase.ExecuteAsync();

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Data!);
        _playerStatisticsRepositoryMock.Verify(
            repository => repository.GetOverallRatingAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
