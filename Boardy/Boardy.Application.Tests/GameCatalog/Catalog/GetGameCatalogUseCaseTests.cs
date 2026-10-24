using Boardy.Application.GameCatalog.Catalog;
using Boardy.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace Boardy.Application.Tests.GameCatalog.Catalog;

public class GetGameCatalogUseCaseTests
{
    private readonly Mock<IGameCatalogRepository> _mockRepository = new();
    private readonly GetGameCatalogUseCase _useCase;

    public GetGameCatalogUseCaseTests()
    {
        var mockLogger = new Mock<ILogger<GetGameCatalogUseCase>>();
        _useCase = new GetGameCatalogUseCase(_mockRepository.Object, mockLogger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithoutFilters_ReturnsAllGamesWithTheirData()
    {
        var monopoly = new Domain.Entities.Game("Монополія", "Економічна гра", 2, 6, GameStatus.Active);
        var carcassonne = new Domain.Entities.Game("Каркассон", "Стратегічна гра", 2, 5, GameStatus.Active);
        _mockRepository.Setup(repository => repository.GetAllAsync())
            .ReturnsAsync(new[] { monopoly, carcassonne });

        var result = await _useCase.ExecuteAsync(new GetGameCatalogRequest(null, null));

        Assert.True(result.IsSuccess);
        var games = result.Data!.ToList();
        Assert.Collection(games,
            game => AssertGame(game, monopoly),
            game => AssertGame(game, carcassonne));
        _mockRepository.Verify(repository => repository.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsGamesMatchingSearchAndPlayersCount()
    {
        var monopoly = new Domain.Entities.Game("Монополія", "Економічна гра", 2, 6, GameStatus.Active);
        var carcassonne = new Domain.Entities.Game("Каркассон", "Стратегічна гра", 2, 5, GameStatus.Active);
        _mockRepository.Setup(repository => repository.GetAllAsync())
            .ReturnsAsync(new[] { monopoly, carcassonne });

        var result = await _useCase.ExecuteAsync(new GetGameCatalogRequest("моноп", 4));

        Assert.True(result.IsSuccess);
        AssertGame(Assert.Single(result.Data!), monopoly);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSearchDoesNotMatch_ReturnsEmptyCatalog()
    {
        _mockRepository.Setup(repository => repository.GetAllAsync())
            .ReturnsAsync(new[]
            {
                new Domain.Entities.Game("Монополія", "Економічна гра", 2, 6, GameStatus.Active)
            });

        var result = await _useCase.ExecuteAsync(new GetGameCatalogRequest("Шахи", null));

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Data!);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPlayersCountDoesNotMatch_ReturnsEmptyCatalog()
    {
        _mockRepository.Setup(repository => repository.GetAllAsync())
            .ReturnsAsync(new[]
            {
                new Domain.Entities.Game("Монополія", "Економічна гра", 2, 6, GameStatus.Active)
            });

        var result = await _useCase.ExecuteAsync(new GetGameCatalogRequest(null, 7));

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Data!);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ExecuteAsync_WithInvalidPlayersCount_ReturnsFailureWithoutLoadingCatalog(int playersCount)
    {
        var result = await _useCase.ExecuteAsync(new GetGameCatalogRequest(null, playersCount));

        Assert.False(result.IsSuccess);
        Assert.Equal("Кількість гравців має бути більшою за 0.", result.ErrorMessage);
        _mockRepository.Verify(repository => repository.GetAllAsync(), Times.Never);
    }

    private static void AssertGame(GameCatalogItem actual, Domain.Entities.Game expected)
    {
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.Name, actual.Name);
        Assert.Equal(expected.Description, actual.Description);
        Assert.Equal(expected.MinPlayers, actual.MinPlayers);
        Assert.Equal(expected.MaxPlayers, actual.MaxPlayers);
        Assert.Equal(expected.Status, actual.Status);
    }
}
