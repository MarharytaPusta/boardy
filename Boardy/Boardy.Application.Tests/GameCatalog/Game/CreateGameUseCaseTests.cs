using Boardy.Application.GameCatalog.Game;
using Boardy.Application.GameCatalog.ScoreTemplate; 
using Boardy.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Boardy.Application.Tests.GameCatalog.Game;

public class CreateGameUseCaseTests
{
    private readonly Mock<IGameRepository> _mockRepository;
    private readonly CreateGameUseCase _useCase;

    private readonly List<CriterionDto> _validCriteria = new()
    {
        new CriterionDto("Бали", false)
    };

    public CreateGameUseCaseTests()
    {
        _mockRepository = new Mock<IGameRepository>();
        var mockLogger = new Mock<ILogger<CreateGameUseCase>>();

        _useCase = new CreateGameUseCase(_mockRepository.Object, mockLogger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidRequest_SavesGameAndReturnsSuccess()
    {
        var criteria = new List<CriterionDto>
        {
            new CriterionDto("Перемога", false),
            new CriterionDto("Штраф", true)
        };
        var request = new CreateGameRequest("Монополія", "Класична економічна гра", 2, 6, GameStatus.Active, criteria);

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
        var request = new CreateGameRequest(invalidName, "Опис", 2, 4, GameStatus.Active, _validCriteria);

        var result = await _useCase.ExecuteAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("Назва гри не може бути порожньою.", result.ErrorMessage);
        _mockRepository.Verify(repo => repo.AddAsync(It.IsAny<Domain.Entities.Game>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenMinPlayersLessThanOne_ReturnsFailure()
    {
        var request = new CreateGameRequest("Шахи", "Опис", 0, 2, GameStatus.Active, _validCriteria);

        var result = await _useCase.ExecuteAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("Мінімальна кількість гравців має бути більше 0.", result.ErrorMessage);
        _mockRepository.Verify(repo => repo.AddAsync(It.IsAny<Domain.Entities.Game>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenMinPlayersGreaterThanMaxPlayers_ReturnsFailure()
    {
        var request = new CreateGameRequest("Манчкін", "Опис", 6, 4, GameStatus.Active, _validCriteria);

        var result = await _useCase.ExecuteAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("Мінімальна кількість гравців не може перевищувати максимальну.", result.ErrorMessage);
        _mockRepository.Verify(repo => repo.AddAsync(It.IsAny<Domain.Entities.Game>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyCriteriaList_ReturnsFailure()
    {
        var emptyCriteria = new List<CriterionDto>();
        var request = new CreateGameRequest("Каркассон", "Опис", 2, 5, GameStatus.Active, emptyCriteria);

        var result = await _useCase.ExecuteAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("Додайте хоча б один критерій оцінювання", result.ErrorMessage);
        _mockRepository.Verify(repo => repo.AddAsync(It.IsAny<Domain.Entities.Game>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyCriterionName_ReturnsFailure()
    {
        var invalidCriteria = new List<CriterionDto>
        {
            new CriterionDto("Правило 1", false),
            new CriterionDto("   ", true)
        };
        var request = new CreateGameRequest("Уно", "Опис", 2, 10, GameStatus.Active, invalidCriteria);

        var result = await _useCase.ExecuteAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("Критерій не може бути порожнім", result.ErrorMessage);
        _mockRepository.Verify(repo => repo.AddAsync(It.IsAny<Domain.Entities.Game>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithDuplicateCriteriaNames_ReturnsFailure()
    {
        var duplicateCriteria = new List<CriterionDto>
        {
            new CriterionDto("Монети", false),
            new CriterionDto("монети ", true)
        };
        var request = new CreateGameRequest("Цитаделі", "Опис", 2, 8, GameStatus.Active, duplicateCriteria);

        var result = await _useCase.ExecuteAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("Критерії в одному шаблоні не можуть мати однакові назви", result.ErrorMessage);
        _mockRepository.Verify(repo => repo.AddAsync(It.IsAny<Domain.Entities.Game>()), Times.Never);
    }
}