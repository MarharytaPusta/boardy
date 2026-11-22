using Boardy.Application.Player;
using Boardy.Application.Statistics;
using Microsoft.Extensions.Logging;
using Moq;

namespace Boardy.Application.Tests.Statistics;

public class GetOverallRatingUseCaseTests
{
    private readonly Mock<IPlayerRepository> _playerRepositoryMock = new();
    private readonly GetOverallRatingUseCase _useCase;

    public GetOverallRatingUseCaseTests()
    {
        var loggerMock = new Mock<ILogger<GetOverallRatingUseCase>>();
        _useCase = new GetOverallRatingUseCase(_playerRepositoryMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsPlayerStatistics()
    {
        var player = CreatePlayer("Олена", playedGamesCount: 3, playedMatchesCount: 5, winsCount: 2);
        _playerRepositoryMock.Setup(repository => repository.GetAllAsync())
            .ReturnsAsync(new[] { player });

        var result = await _useCase.ExecuteAsync();

        Assert.True(result.IsSuccess);
        var ratingItem = Assert.Single(result.Data!);
        Assert.Equal(player.Id, ratingItem.PlayerId);
        Assert.Equal("Олена", ratingItem.PlayerName);
        Assert.Equal(3, ratingItem.PlayedGamesCount);
        Assert.Equal(5, ratingItem.PlayedMatchesCount);
        Assert.Equal(2, ratingItem.WinsCount);
    }

    [Fact]
    public async Task ExecuteAsync_SortsRatingByWinsMatchesGamesAndName()
    {
        var firstPlayer = CreatePlayer("Богдан", playedGamesCount: 2, playedMatchesCount: 4, winsCount: 2);
        var secondPlayer = CreatePlayer("Анна", playedGamesCount: 1, playedMatchesCount: 4, winsCount: 2);
        var thirdPlayer = CreatePlayer("Віра", playedGamesCount: 3, playedMatchesCount: 5, winsCount: 1);
        var fourthPlayer = CreatePlayer("Ганна", playedGamesCount: 1, playedMatchesCount: 4, winsCount: 2);
        _playerRepositoryMock.Setup(repository => repository.GetAllAsync())
            .ReturnsAsync(new[] { thirdPlayer, firstPlayer, fourthPlayer, secondPlayer });

        var result = await _useCase.ExecuteAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal(
            new[] { "Богдан", "Анна", "Ганна", "Віра" },
            result.Data!.Select(item => item.PlayerName));
    }

    [Fact]
    public async Task ExecuteAsync_WithNoPlayers_ReturnsEmptyRating()
    {
        _playerRepositoryMock.Setup(repository => repository.GetAllAsync())
            .ReturnsAsync(Array.Empty<Domain.Entities.Player>());

        var result = await _useCase.ExecuteAsync();

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Data!);
        _playerRepositoryMock.Verify(repository => repository.GetAllAsync(), Times.Once);
    }

    private static Domain.Entities.Player CreatePlayer(
        string name,
        int playedGamesCount,
        int playedMatchesCount,
        int winsCount)
    {
        var player = new Domain.Entities.Player(name);
        player.UpdateStatistics(playedGamesCount, playedMatchesCount, winsCount);
        return player;
    }
}
