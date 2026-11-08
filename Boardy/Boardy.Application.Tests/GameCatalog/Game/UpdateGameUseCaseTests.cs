using Boardy.Application.GameCatalog.Game;
using Boardy.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Boardy.Application.Tests.GameCatalog.Game;

public class UpdateGameUseCaseTests
{
    private readonly Mock<IGameRepository> _mockRepository;
    private readonly Mock<ILogger<UpdateGameUseCase>> _mockLogger;
    private readonly UpdateGameUseCase _useCase;


    public UpdateGameUseCaseTests()
    {
        _mockRepository = new Mock<IGameRepository>();
        _mockLogger = new Mock<ILogger<UpdateGameUseCase>>();
        _useCase = new UpdateGameUseCase(_mockRepository.Object, _mockLogger.Object);
    }


    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task ExecuteAsync_WhenNameIsNullOrWhitespace_ReturnsFailure(string invalidName)
    {
        var request = new UpdateGameRequest(Guid.NewGuid(), invalidName, "Опис", 2, 4);

        var result = await _useCase.ExecuteAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("Назва гри не може бути порожньою.", result.ErrorMessage);

        _mockRepository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
    }


    [Theory]
    [InlineData(0, 4)]
    [InlineData(5, 2)]
    public async Task ExecuteAsync_WhenPlayersCountIsInvalid_ReturnsFailure(int min, int max)
    {
        var request = new UpdateGameRequest(Guid.NewGuid(), "Нормальна Назва", "Опис", min, max);

        var result = await _useCase.ExecuteAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("Некоректно вказана кількість гравців.", result.ErrorMessage);
    }


    [Fact]
    public async Task ExecuteAsync_WhenGameNotFound_ReturnsFailure()
    {
        var request = new UpdateGameRequest(Guid.NewGuid(), "Нова Назва", "Новий Опис", 2, 4);

        _mockRepository
            .Setup(repo => repo.GetByIdAsync(request.GameId))
            .Returns(Task.FromResult<Boardy.Domain.Entities.Game?>(null));

        var result = await _useCase.ExecuteAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("Гру не знайдено.", result.ErrorMessage);
    }


    [Fact]
    public async Task ExecuteAsync_WhenValidRequest_UpdatesGameAndReturnsSuccess()
    {
        var existingGame = new Boardy.Domain.Entities.Game("Стара Назва", "Старий Опис", 1, 2, GameStatus.Active);

        var request = new UpdateGameRequest(existingGame.Id, "Нова Назва", "Новий Опис", 3, 6);

        _mockRepository
            .Setup(repo => repo.GetByIdAsync(request.GameId))
            .ReturnsAsync(existingGame);

        var result = await _useCase.ExecuteAsync(request);

        Assert.True(result.IsSuccess);

        Assert.Equal("Нова Назва", existingGame.Name);
        Assert.Equal("Новий Опис", existingGame.Description);
        Assert.Equal(3, existingGame.MinPlayers);
        Assert.Equal(6, existingGame.MaxPlayers);

        Assert.Equal(GameStatus.Active, existingGame.Status);

        _mockRepository.Verify(repo => repo.UpdateAsync(existingGame), Times.Once);
    }
}