using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Boardy.Application.Statistics;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Boardy.Application.Tests.Statistics;
public class GetGameStatisticsUseCaseTests
{
    private readonly Mock<IGameStatisticsRepository> _repositoryMock;
    private readonly Mock<ILogger<GetGameStatisticsUseCase>> _loggerMock;
    private readonly GetGameStatisticsUseCase _useCase;

    public GetGameStatisticsUseCaseTests()
    {
        _repositoryMock = new Mock<IGameStatisticsRepository>();
        _loggerMock = new Mock<ILogger<GetGameStatisticsUseCase>>();
        _useCase = new GetGameStatisticsUseCase(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WhenGameIdIsEmpty_ReturnsFailureAndDoesNotCallRepository()
    {
        var result = await _useCase.ExecuteAsync(Guid.Empty);

        Assert.False(result.IsSuccess);
        Assert.Equal("Некоректний ідентифікатор гри.", result.ErrorMessage);
        _repositoryMock.Verify(
            r => r.GetByGameIdAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task ExecuteAsync_WhenTopResultsLimitIsZeroOrNegative_UsesDefaultLimitThree(int invalidLimit)
    {
        var gameId = Guid.NewGuid();
        var expectedDto = new GameStatisticsDto(
            gameId,
            "Дивокрай",
            TotalPlayedMatches: 0,
            TopResults: Array.Empty<TopScoreDto>(),
            Players: Array.Empty<GamePlayerStatDto>());

        _repositoryMock
            .Setup(r => r.GetByGameIdAsync(gameId, 3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        var result = await _useCase.ExecuteAsync(gameId, topResultsLimit: invalidLimit);

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedDto, result.Data);
        _repositoryMock.Verify(
            r => r.GetByGameIdAsync(gameId, 3, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenGameNotFound_ReturnsFailure()
    {
        var gameId = Guid.NewGuid();
        _repositoryMock
            .Setup(r => r.GetByGameIdAsync(gameId, 3, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GameStatisticsDto?)null);

        var result = await _useCase.ExecuteAsync(gameId);

        Assert.False(result.IsSuccess);
        Assert.Equal("Гру не знайдено.", result.ErrorMessage);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task ExecuteAsync_WhenStatisticsExist_ReturnsSuccessWithData()
    {
        var gameId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var expectedDto = new GameStatisticsDto(
            gameId,
            "Крила",
            TotalPlayedMatches: 4,
            TopResults: new List<TopScoreDto>
            {
                new(playerId, "Маргарита", false, 112, DateTime.UtcNow, "Базовий")
            },
            Players: new List<GamePlayerStatDto>
            {
                new(playerId, "Маргарита", false, MatchesPlayedInGame: 4, WinsInGame: 3, BestScoreInGame: 112)
            });

        _repositoryMock
            .Setup(r => r.GetByGameIdAsync(gameId, 3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        var result = await _useCase.ExecuteAsync(gameId);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("Крила", result.Data.GameName);
        Assert.Equal(4, result.Data.TotalPlayedMatches);
        Assert.Single(result.Data.TopResults);
        Assert.Single(result.Data.Players);
    }
}
