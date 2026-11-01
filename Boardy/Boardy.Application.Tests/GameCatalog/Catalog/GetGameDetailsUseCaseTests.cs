using Boardy.Application.GameCatalog.Catalog;
using Boardy.Application.GameCatalog.Game;
using Boardy.Domain.Entities;
using Boardy.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Boardy.Application.Tests.GameCatalog.Catalog;

public class GetGameDetailsUseCaseTests
{
    private readonly Mock<IGameRepository> _mockRepository;
    private readonly Mock<ILogger<GetGameDetailsUseCase>> _mockLogger;
    private readonly GetGameDetailsUseCase _useCase;

    public GetGameDetailsUseCaseTests()
    {
        _mockRepository = new Mock<IGameRepository>();
        _mockLogger = new Mock<ILogger<GetGameDetailsUseCase>>();
        _useCase = new GetGameDetailsUseCase(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WhenGameNotFound_ReturnsFailure()
    {
        var request = new GetGameDetailsRequest(Guid.NewGuid());

        _mockRepository
            .Setup(repo => repo.GetByIdAsync(request.GameId))
            .Returns(Task.FromResult<Boardy.Domain.Entities.Game?>(null));

        var result = await _useCase.ExecuteAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("Гру не знайдено в базі даних.", result.ErrorMessage);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task ExecuteAsync_WhenGameExists_ReturnsSuccessWithMappedGameDetailsDto()
    {
        var game = new Boardy.Domain.Entities.Game("Крила", "Гра про птахів", 1, 5, GameStatus.Active);
            
            var template = new Boardy.Domain.Entities.ScoreTemplate("Базовий шаблон");
            template.AddCriterion(new Boardy.Domain.Entities.ScoreCriterion("Птахи", false));
            template.AddCriterion(new Boardy.Domain.Entities.ScoreCriterion("Мінус за карти", true));

        game.AddScoreTemplate(template);

        var request = new GetGameDetailsRequest(game.Id);

        _mockRepository
            .Setup(repo => repo.GetByIdAsync(request.GameId))
            .ReturnsAsync(game);

        var result = await _useCase.ExecuteAsync(request);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);

        var dto = result.Data;

        Assert.Equal(game.Id, dto.Id);
        Assert.Equal("Крила", dto.Name);
        Assert.Equal("Гра про птахів", dto.Description);
        Assert.Equal(1, dto.MinPlayers);
        Assert.Equal(5, dto.MaxPlayers);

        Assert.Single(dto.Templates);
        var templateDto = dto.Templates.First();
        Assert.Equal("Базовий шаблон", templateDto.Name);

        Assert.Equal(2, templateDto.Criteria.Count());

        var criteriaList = templateDto.Criteria.ToList();
        Assert.Equal("Птахи", criteriaList[0].Name);
        Assert.False(criteriaList[0].IsNegative);

        Assert.Equal("Мінус за карти", criteriaList[1].Name);
        Assert.True(criteriaList[1].IsNegative);
    }
}