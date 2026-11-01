using Boardy.Application.Scoring;
using Boardy.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Boardy.Application.Tests.Scoring.CreateMatch;

public class CreateMatchUseCaseTests
{
    private readonly Mock<IMatchRepository> _matchRepositoryMock;
    private readonly Mock<ILogger<CreateMatchUseCase>> _loggerMock;
    private readonly CreateMatchUseCase _useCase;

    public CreateMatchUseCaseTests()
    {
        _matchRepositoryMock = new Mock<IMatchRepository>();
        _loggerMock = new Mock<ILogger<CreateMatchUseCase>>();

        _useCase = new CreateMatchUseCase(_matchRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidData_CreatesMatchAndReturnsSuccess()
    {
        var scoreTemplateId = Guid.NewGuid();
        var playerIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var request = new CreateMatchRequest(scoreTemplateId, playerIds);

        var result = await _useCase.ExecuteAsync(request);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Data);

        _matchRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Domain.Entities.Match>()), Times.Once);

        _matchRepositoryMock.Verify(repo => repo.AddAsync(It.Is<Domain.Entities.Match>(m =>
            m.ScoreTemplateId == scoreTemplateId &&
            m.Participants.Count == 2 &&
            m.Participants.Any(p => p.PlayerId == playerIds[0]) &&
            m.Participants.Any(p => p.PlayerId == playerIds[1])
        )), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyScoreTemplateId_ReturnsFailure()
    {
        var request = new CreateMatchRequest(Guid.Empty, new List<Guid> { Guid.NewGuid() });

        var result = await _useCase.ExecuteAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("Шаблон гри не знайдено.", result.ErrorMessage);

        _matchRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Domain.Entities.Match>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithNullPlayerIds_ReturnsFailure()
    {
        var request = new CreateMatchRequest(Guid.NewGuid(), null!);

        var result = await _useCase.ExecuteAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("Для початку партії необхідно обрати хоча б одного гравця.", result.ErrorMessage);
        _matchRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Domain.Entities.Match>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyPlayerIds_ReturnsFailure()
    {
        var request = new CreateMatchRequest(Guid.NewGuid(), new List<Guid>());

        var result = await _useCase.ExecuteAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("Для початку партії необхідно обрати хоча б одного гравця.", result.ErrorMessage);
        _matchRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Domain.Entities.Match>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithDuplicatePlayerIds_ReturnsFailure()
    {
        var duplicatePlayerId = Guid.NewGuid();
        var request = new CreateMatchRequest(Guid.NewGuid(), new List<Guid> { duplicatePlayerId, duplicatePlayerId });

        var result = await _useCase.ExecuteAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("Один і той самий гравець не може бути доданий до партії двічі.", result.ErrorMessage);
        _matchRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Domain.Entities.Match>()), Times.Never);
    }
}