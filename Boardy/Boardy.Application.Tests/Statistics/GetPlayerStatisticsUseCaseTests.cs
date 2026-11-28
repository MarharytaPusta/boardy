using Boardy.Application.GameCatalog.Game;
using Boardy.Application.Player;
using Boardy.Application.Scoring;
using Boardy.Application.Statistics;
using Boardy.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using DomainMatch = Boardy.Domain.Entities.Match;
using DomainPlayer = Boardy.Domain.Entities.Player;

namespace Boardy.Application.Tests.Statistics;

public class GetPlayerStatisticsUseCaseTests
{
    private readonly Mock<IPlayerRepository> _playerRepositoryMock = new();
    private readonly Mock<IPlayerStatisticsRepository> _playerStatisticsRepositoryMock = new();
    private readonly GetPlayerStatisticsUseCase _useCase;

    public GetPlayerStatisticsUseCaseTests()
    {
        var loggerMock = new Mock<ILogger<GetPlayerStatisticsUseCase>>();
        _useCase = new GetPlayerStatisticsUseCase(
            _playerRepositoryMock.Object,
            _playerStatisticsRepositoryMock.Object,
            loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsPlayerAndAggregatedGameStatistics()
    {
        var player = new DomainPlayer("Олена");
        var gameStatistics = new[]
        {
            new PlayerGameStatisticsDto("Каркасон", PlayedMatchesCount: 1, WinsCount: 1),
            new PlayerGameStatisticsDto("Крила", PlayedMatchesCount: 2, WinsCount: 1)
        };

        _playerRepositoryMock
            .Setup(repository => repository.GetByIdAsync(player.Id))
            .ReturnsAsync(player);
        _playerStatisticsRepositoryMock
            .Setup(repository => repository.GetPlayerGamesStatisticsAsync(player.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(gameStatistics);

        var result = await _useCase.ExecuteAsync(player.Id);

        Assert.True(result.IsSuccess);
        var statistics = result.Data!;
        Assert.Equal(player.Id, statistics.PlayerId);
        Assert.Equal("Олена", statistics.PlayerName);
        Assert.Equal(2, statistics.PlayedGamesCount);
        Assert.Equal(3, statistics.PlayedMatchesCount);
        Assert.Equal(2, statistics.WinsCount);
        Assert.Collection(
            statistics.Games,
            game =>
            {
                Assert.Equal("Каркасон", game.GameName);
                Assert.Equal(1, game.PlayedMatchesCount);
                Assert.Equal(1, game.WinsCount);
            },
            game =>
            {
                Assert.Equal("Крила", game.GameName);
                Assert.Equal(2, game.PlayedMatchesCount);
                Assert.Equal(1, game.WinsCount);
            });
    }

    [Fact]
    public async Task ExecuteAsync_WhenPlayerIdIsEmpty_ReturnsFailure()
    {
        var result = await _useCase.ExecuteAsync(Guid.Empty);

        Assert.False(result.IsSuccess);
        Assert.Equal("Некоректний ідентифікатор гравця.", result.ErrorMessage);
        _playerRepositoryMock.Verify(
            repository => repository.GetByIdAsync(It.IsAny<Guid>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPlayerDoesNotExist_ReturnsFailure()
    {
        var playerId = Guid.NewGuid();
        _playerRepositoryMock
            .Setup(repository => repository.GetByIdAsync(playerId))
            .ReturnsAsync((DomainPlayer?)null);

        var result = await _useCase.ExecuteAsync(playerId);

        Assert.False(result.IsSuccess);
        Assert.Equal("Гравця не знайдено.", result.ErrorMessage);
        _playerStatisticsRepositoryMock.Verify(
            repository => repository.GetPlayerGamesStatisticsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPlayerIsDeleted_ReturnsFailure()
    {
        var player = new DomainPlayer("Олена");
        player.MarkAsDeleted();
        _playerRepositoryMock
            .Setup(repository => repository.GetByIdAsync(player.Id))
            .ReturnsAsync(player);

        var result = await _useCase.ExecuteAsync(player.Id);

        Assert.False(result.IsSuccess);
        Assert.Equal("Гравця не знайдено.", result.ErrorMessage);
        _playerStatisticsRepositoryMock.Verify(
            repository => repository.GetPlayerGamesStatisticsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithNoPlayedMatches_ReturnsEmptyGamesAndZeroTotals()
    {
        var player = new DomainPlayer("Тарас");
        _playerRepositoryMock
            .Setup(repository => repository.GetByIdAsync(player.Id))
            .ReturnsAsync(player);
        _playerStatisticsRepositoryMock
            .Setup(repository => repository.GetPlayerGamesStatisticsAsync(player.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<PlayerGameStatisticsDto>());

        var result = await _useCase.ExecuteAsync(player.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.Data!.PlayedGamesCount);
        Assert.Equal(0, result.Data.PlayedMatchesCount);
        Assert.Equal(0, result.Data.WinsCount);
        Assert.Empty(result.Data.Games);
    }
}
