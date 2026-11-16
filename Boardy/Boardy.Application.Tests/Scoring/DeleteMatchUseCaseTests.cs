using Boardy.Application.Authentication.Admin;
using Boardy.Application.Scoring;
using Boardy.Application.Shared;
using Boardy.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Boardy.Tests.Application.Scoring;

public class DeleteMatchUseCaseTests
{
    private readonly Mock<IMatchRepository> _mockMatchRepository;
    private readonly Mock<IUserSession> _mockUserSession;
    private readonly Mock<ILogger<DeleteMatchUseCase>> _mockLogger;
    private readonly DeleteMatchUseCase _useCase;

    public DeleteMatchUseCaseTests()
    {
        _mockMatchRepository = new Mock<IMatchRepository>();
        _mockUserSession = new Mock<IUserSession>();
        _mockLogger = new Mock<ILogger<DeleteMatchUseCase>>();

        _useCase = new DeleteMatchUseCase(
            _mockMatchRepository.Object,
            _mockUserSession.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailureAndLogWarning_WhenUserIsNotAdmin()
    {
        _mockUserSession.Setup(s => s.IsAdminMode).Returns(false);
        var request = new DeleteMatchRequest(Guid.NewGuid());

        var result = await _useCase.ExecuteAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("Видаляти партії може лише адміністратор.", result.ErrorMessage);
        _mockMatchRepository.Verify(repo => repo.DeleteAsync(It.IsAny<Domain.Entities.Match>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailureAndLogWarning_WhenMatchIdIsEmpty()
    {
        _mockUserSession.Setup(s => s.IsAdminMode).Returns(true);
        var request = new DeleteMatchRequest(Guid.Empty);

        var result = await _useCase.ExecuteAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("Некоректний ідентифікатор партії.", result.ErrorMessage);
        _mockMatchRepository.Verify(repo => repo.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        _mockMatchRepository.Verify(repo => repo.DeleteAsync(It.IsAny<Domain.Entities.Match>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnFailureAndLogWarning_WhenMatchDoesNotExist()
    {
        _mockUserSession.Setup(s => s.IsAdminMode).Returns(true);
        var matchId = Guid.NewGuid();
        _mockMatchRepository.Setup(repo => repo.GetByIdAsync(matchId))
                            .ReturnsAsync((Domain.Entities.Match?)null);

        var request = new DeleteMatchRequest(matchId);

        var result = await _useCase.ExecuteAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("Партію не знайдено.", result.ErrorMessage);
        _mockMatchRepository.Verify(repo => repo.DeleteAsync(It.IsAny<Domain.Entities.Match>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldDeleteMatchAndReturnSuccess_WhenRequestIsValid()
    {
        _mockUserSession.Setup(s => s.IsAdminMode).Returns(true);
        var matchId = Guid.NewGuid();
        var match = new Domain.Entities.Match(Guid.NewGuid());

        _mockMatchRepository.Setup(repo => repo.GetByIdAsync(matchId))
                            .ReturnsAsync(match);

        var request = new DeleteMatchRequest(matchId);

        var result = await _useCase.ExecuteAsync(request);

        Assert.True(result.IsSuccess);
        _mockMatchRepository.Verify(repo => repo.DeleteAsync(match), Times.Once);
    }
}