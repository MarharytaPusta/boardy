using Boardy.Application.GameCatalog.Catalog;
using Boardy.Application.GameCatalog.Game;
using Boardy.Application.Scoring;
using Boardy.Domain.Entities;
using Boardy.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace Boardy.Application.Tests.Statistics;

public class GetGamesStatisticsUseCaseTests
{
    private readonly Mock<IGameRepository> _mockGameRepository;
    private readonly Mock<IMatchRepository> _mockMatchRepository;
    private readonly Mock<ILogger<GetGamesStatisticsUseCase>> _mockLogger;
    private readonly GetGamesStatisticsUseCase _useCase;

    public GetGamesStatisticsUseCaseTests()
    {
        _mockGameRepository = new Mock<IGameRepository>();
        _mockMatchRepository = new Mock<IMatchRepository>();
        _mockLogger = new Mock<ILogger<GetGamesStatisticsUseCase>>();

        _useCase = new GetGamesStatisticsUseCase(
            _mockGameRepository.Object,
            _mockMatchRepository.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnNoData_WhenNoGamesExist()
    {
        _mockGameRepository.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(new List<Game>());
        _mockMatchRepository.Setup(repo => repo.GetMatchesCountByGameAsync())
            .ReturnsAsync(new Dictionary<Guid, int>());

        var result = await _useCase.ExecuteAsync();

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("Немає даних", result.Data.MostPlayedGame);
        Assert.Equal("Немає даних", result.Data.LeastPlayedGame);
        Assert.Empty(result.Data.MatchesCountByGame);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnNoData_WhenNoMatchesHaveBeenPlayed()
    {
        var game1 = new Game("Catan", "Desc", 3, 4, GameStatus.Active);
        var game2 = new Game("Carcassonne", "Desc", 2, 5, GameStatus.Active);

        _mockGameRepository.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(new List<Game> { game1, game2 });
        _mockMatchRepository.Setup(repo => repo.GetMatchesCountByGameAsync())
            .ReturnsAsync(new Dictionary<Guid, int>());

        var result = await _useCase.ExecuteAsync();

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("Немає даних", result.Data.MostPlayedGame);
        Assert.Equal("Немає даних", result.Data.LeastPlayedGame);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldExcludeInactiveGamesByDefault_AndTreatMissingCountsAsZero()
    {
        var activePopular = new Game("Catan", "Desc", 3, 4, GameStatus.Active);
        var activeUnplayed = new Game("Dixit", "Desc", 3, 6, GameStatus.Active);
        var inactiveMostPlayed = new Game("Monopoly", "Desc", 2, 6, GameStatus.Inactive);

        var counts = new Dictionary<Guid, int>
        {
            { activePopular.Id, 5 },
            { inactiveMostPlayed.Id, 12 }
        };

        _mockGameRepository.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(new List<Game> { activePopular, activeUnplayed, inactiveMostPlayed });
        _mockMatchRepository.Setup(repo => repo.GetMatchesCountByGameAsync())
            .ReturnsAsync(counts);

        var result = await _useCase.ExecuteAsync(includeInactive: false);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("Catan (5)", result.Data.MostPlayedGame);
        Assert.Equal("Dixit (0)", result.Data.LeastPlayedGame);
        Assert.Same(counts, result.Data.MatchesCountByGame);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldIncludeInactiveGames_WhenIncludeInactiveIsTrue()
    {
        var activeGame = new Game("Catan", "Desc", 3, 4, GameStatus.Active);
        var inactivePopular = new Game("Monopoly", "Desc", 2, 6, GameStatus.Inactive);
        var inactiveUnplayed = new Game("Alias", "Desc", 4, 12, GameStatus.Inactive);

        var counts = new Dictionary<Guid, int>
        {
            { activeGame.Id, 5 },
            { inactivePopular.Id, 12 }
        };

        _mockGameRepository.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(new List<Game> { activeGame, inactivePopular, inactiveUnplayed });
        _mockMatchRepository.Setup(repo => repo.GetMatchesCountByGameAsync())
            .ReturnsAsync(counts);

        var result = await _useCase.ExecuteAsync(includeInactive: true);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("Monopoly (12)", result.Data.MostPlayedGame);
        Assert.Equal("Alias (0)", result.Data.LeastPlayedGame);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldBreakTiesAlphabetically_WhenGamesHaveEqualMatchesCount()
    {
        var gameB = new Game("Ticket to Ride", "Desc", 2, 5, GameStatus.Active);
        var gameA = new Game("Azul", "Desc", 2, 4, GameStatus.Active);

        var counts = new Dictionary<Guid, int>
        {
            { gameB.Id, 4 },
            { gameA.Id, 4 }
        };

        _mockGameRepository.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(new List<Game> { gameB, gameA });
        _mockMatchRepository.Setup(repo => repo.GetMatchesCountByGameAsync())
            .ReturnsAsync(counts);

        var result = await _useCase.ExecuteAsync();

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("Azul (4)", result.Data.MostPlayedGame);
        Assert.Equal("Azul (4)", result.Data.LeastPlayedGame);
    }

}
