using Boardy.Application.GameCatalog.Game;
using Boardy.Application.GameCatalog.ScoreTemplate;
using Boardy.Domain.Entities;
using Boardy.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Boardy.Application.Tests.GameCatalog.ScoreTemplate;
public class UpdateScoreTemplateUseCaseTests
{
    private readonly Mock<IGameRepository> _gameRepositoryMock;
    private readonly Mock<ILogger<UpdateScoreTemplateUseCase>> _loggerMock;
    private readonly UpdateScoreTemplateUseCase _useCase;

    public UpdateScoreTemplateUseCaseTests()
    {
        _gameRepositoryMock = new Mock<IGameRepository>();
        _loggerMock = new Mock<ILogger<UpdateScoreTemplateUseCase>>();
        _useCase = new UpdateScoreTemplateUseCase(_gameRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ValidRequest_UpdatesTemplateAndCallsRepository()
    {
        var game = CreateGameWithTemplate(out var templateId);
        _gameRepositoryMock.Setup(r => r.GetByIdAsync(game.Id)).ReturnsAsync(game);

        var newCriteria = new List<CriterionDto>
        {
            new("Новий критерій 1", false),
            new("Штраф", true)
        };
        var request = new UpdateScoreTemplateRequest(game.Id, templateId, "  Оновлена назва  ", newCriteria);

        var result = await _useCase.ExecuteAsync(request);

        Assert.True(result.IsSuccess);

        var updatedTemplate = game.ScoreTemplates.First(t => t.Id == templateId);
        Assert.Equal("Оновлена назва", updatedTemplate.Name);
        Assert.Equal(2, updatedTemplate.Criteria.Count);
        Assert.Contains(updatedTemplate.Criteria, c => c.Name == "Новий критерій 1" && !c.IsNegative);
        Assert.Contains(updatedTemplate.Criteria, c => c.Name == "Штраф" && c.IsNegative);

        _gameRepositoryMock.Verify(r => r.UpdateAsync(game), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_GameNotFound_ReturnsFailure()
    {
        var gameId = Guid.NewGuid();
        _gameRepositoryMock.Setup(r => r.GetByIdAsync(gameId)).ReturnsAsync((Boardy.Domain.Entities.Game?)null);

        var request = new UpdateScoreTemplateRequest(
            gameId,
            Guid.NewGuid(),
            "Назва",
            new List<CriterionDto> { new("Критерій", false) });

        var result = await _useCase.ExecuteAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("Гру не знайдено.", result.ErrorMessage);
        _gameRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Boardy.Domain.Entities.Game>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_TemplateNotFound_ReturnsFailure()
    {
        var game = CreateGameWithTemplate(out _);
        _gameRepositoryMock.Setup(r => r.GetByIdAsync(game.Id)).ReturnsAsync(game);

        var nonExistentTemplateId = Guid.NewGuid();
        var request = new UpdateScoreTemplateRequest(
            game.Id,
            nonExistentTemplateId,
            "Назва",
            new List<CriterionDto> { new("Критерій", false) });

        var result = await _useCase.ExecuteAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("Шаблон оцінювання не знайдено.", result.ErrorMessage);
        _gameRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Boardy.Domain.Entities.Game>()), Times.Never);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ExecuteAsync_EmptyTemplateName_ReturnsFailure(string invalidName)
    {
        var request = new UpdateScoreTemplateRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            invalidName,
            new List<CriterionDto> { new("Критерій", false) });

        var result = await _useCase.ExecuteAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("Назва шаблону не може бути порожньою.", result.ErrorMessage);
    }

    [Fact]
    public async Task ExecuteAsync_EmptyCriteriaList_ReturnsFailure()
    {
        var request = new UpdateScoreTemplateRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Валідна назва",
            new List<CriterionDto>());

        var result = await _useCase.ExecuteAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("Додайте хоча б один критерій оцінювання.", result.ErrorMessage);
    }

    [Fact]
    public async Task ExecuteAsync_DuplicateCriteriaNames_CaseInsensitive_ReturnsFailure()
    {
        var request = new UpdateScoreTemplateRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Валідна назва",
            new List<CriterionDto>
            {
                new("Птахи", false),
                new("птахи ", true)
            });

        var result = await _useCase.ExecuteAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("Критерії в одному шаблоні не можуть мати однакові назви.", result.ErrorMessage);
    }

    private static Boardy.Domain.Entities.Game CreateGameWithTemplate(out Guid templateId)
    {
        var game = new Boardy.Domain.Entities.Game("Крила", "Опис гри", 1, 5, GameStatus.Active);
        var template = new Boardy.Domain.Entities.ScoreTemplate("Базовий");
        template.AddCriterion(new ScoreCriterion("Старий критерій", false));
        game.AddScoreTemplate(template);

        templateId = template.Id;
        return game;
    }
}