using Boardy.Application.GameCatalog.Game;
using Boardy.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace Boardy.Application.Tests.GameCatalog.Game;

public class DeleteGameUseCaseTests
{
    private readonly Mock<IGameRepository> _mockRepository;
    private readonly DeleteGameUseCase _useCase;

    public DeleteGameUseCaseTests()
    {
        _mockRepository = new Mock<IGameRepository>();
        var mockLogger = new Mock<ILogger<DeleteGameUseCase>>();

        _useCase = new DeleteGameUseCase(_mockRepository.Object, mockLogger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WhenGameDoesNotExist_ReturnsFailure()
    {
        var gameId = Guid.NewGuid();
        var request = new DeleteGameRequest(gameId);

        _mockRepository.Setup(repository => repository.GetByIdAsync(gameId))
            .ReturnsAsync((Domain.Entities.Game?)null);

        var result = await _useCase.ExecuteAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("Гру не знайдено.", result.ErrorMessage);
        _mockRepository.Verify(repository => repository.DeleteAsync(It.IsAny<Domain.Entities.Game>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenGameExists_DeletesGameAndReturnsSuccess()
    {
        var game = new Domain.Entities.Game("Шахи", "Класична стратегічна гра", 2, 2, GameStatus.Active);
        var request = new DeleteGameRequest(game.Id);

        _mockRepository.Setup(repository => repository.GetByIdAsync(game.Id))
            .ReturnsAsync(game);

        var result = await _useCase.ExecuteAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Null(result.ErrorMessage);
        _mockRepository.Verify(repository => repository.DeleteAsync(game), Times.Once);
    }
}
