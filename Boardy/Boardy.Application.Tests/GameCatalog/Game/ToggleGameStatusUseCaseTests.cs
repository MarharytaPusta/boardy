using Boardy.Application.GameCatalog.Game;
using Boardy.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace Boardy.Application.Tests.GameCatalog.Game;

public class ToggleGameStatusUseCaseTests
{
    private readonly Mock<IGameRepository> _mockGameRepository;
    private readonly ToggleGameStatusUseCase _useCase;
    private readonly Mock<ILogger<ToggleGameStatusUseCase>> _mockLogger;

    public ToggleGameStatusUseCaseTests()
    {
        _mockGameRepository = new Mock<IGameRepository>();
        _mockLogger = new Mock<ILogger<ToggleGameStatusUseCase>>();
        _useCase = new ToggleGameStatusUseCase(_mockGameRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailure_WhenGameDoesNotExist()
    {
        var nonExistentGameId = Guid.NewGuid();
        _mockGameRepository.Setup(repo => repo.GetByIdAsync(nonExistentGameId))
                           .ReturnsAsync((Domain.Entities.Game?)null);

        var request = new ToggleGameStatusRequest(nonExistentGameId);

        var result = await _useCase.ExecuteAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("Гру не знайдено.", result.ErrorMessage);
        _mockGameRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Domain.Entities.Game>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldChangeActiveToInactive_WhenGameIsActive()
    {
        var game = new Domain.Entities.Game("Test Game", "Desc", 2, 4, GameStatus.Active);
        _mockGameRepository.Setup(repo => repo.GetByIdAsync(game.Id))
                           .ReturnsAsync(game);

        var request = new ToggleGameStatusRequest(game.Id);

        var result = await _useCase.ExecuteAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal(GameStatus.Inactive, game.Status);
        _mockGameRepository.Verify(repo => repo.UpdateAsync(game), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldChangeInactiveToActive_WhenGameIsInactive()
    {
        var game = new Domain.Entities.Game("Test Game", "Desc", 2, 4, GameStatus.Inactive);
        _mockGameRepository.Setup(repo => repo.GetByIdAsync(game.Id))
                           .ReturnsAsync(game);

        var request = new ToggleGameStatusRequest(game.Id);

        var result = await _useCase.ExecuteAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal(GameStatus.Active, game.Status);
        _mockGameRepository.Verify(repo => repo.UpdateAsync(game), Times.Once);
    }
}
