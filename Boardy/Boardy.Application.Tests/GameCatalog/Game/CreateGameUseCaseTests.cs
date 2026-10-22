using Boardy.Application.GameCatalog.Game;
using Boardy.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boardy.Application.Tests.GameCatalog.Game;

public class CreateGameUseCaseTests
{
    private readonly Mock<IGameRepository> _mockRepository;
    private readonly CreateGameUseCase _useCase;

    public CreateGameUseCaseTests()
    {
        _mockRepository = new Mock<IGameRepository>();
        var mockLogger = new Mock<ILogger<CreateGameUseCase>>();

        _useCase = new CreateGameUseCase(_mockRepository.Object, mockLogger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_SavesGameAndReturnsSuccess()
    {
        var request = new CreateGameRequest("Монополія", "Класична економічна гра", 2, 6, GameStatus.Active);

        var result = await _useCase.ExecuteAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Null(result.ErrorMessage);

        _mockRepository.Verify(repo => repo.AddAsync(
            It.Is<Domain.Entities.Game>(g =>
                g.Name == request.Name &&
                g.MinPlayers == request.MinPlayers &&
                g.MaxPlayers == request.MaxPlayers &&
                g.Status == request.Status)),
            Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task ExecuteAsync_WithEmptyName_ReturnsFailure(string invalidName)
    {
        var request = new CreateGameRequest(invalidName, "Опис", 2, 4, GameStatus.Active);

        var result = await _useCase.ExecuteAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("Назва гри не може бути порожньою.", result.ErrorMessage);
        _mockRepository.Verify(repo => repo.AddAsync(It.IsAny<Domain.Entities.Game>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenMinPlayersLessThanOne_ReturnsFailure()
    {
        var request = new CreateGameRequest("Шахи", "Опис", 0, 2, GameStatus.Active);

        var result = await _useCase.ExecuteAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("Мінімальна кількість гравців має бути більше 0.", result.ErrorMessage);
        _mockRepository.Verify(repo => repo.AddAsync(It.IsAny<Domain.Entities.Game>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenMinPlayersGreaterThanMaxPlayers_ReturnsFailure()
    {
        var request = new CreateGameRequest("Манчкін", "Опис", 6, 4, GameStatus.Active);

        var result = await _useCase.ExecuteAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("Мінімальна кількість гравців не може перевищувати максимальну.", result.ErrorMessage);
        _mockRepository.Verify(repo => repo.AddAsync(It.IsAny<Domain.Entities.Game>()), Times.Never);
    }
}
