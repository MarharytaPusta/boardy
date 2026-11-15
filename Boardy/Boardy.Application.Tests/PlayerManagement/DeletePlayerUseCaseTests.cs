using Boardy.Application.Player;
using Boardy.Application.Scoring;
using Boardy.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boardy.Application.Tests.PlayerManagement;

public class DeletePlayerUseCaseTests
{
    private readonly Mock<IPlayerRepository> _mockRepository;
    private readonly Mock<IMatchRepository> _matchRepositoryMock;
    private readonly DeletePlayerUseCase _useCase;

    public DeletePlayerUseCaseTests()
    {
        _mockRepository = new Mock<IPlayerRepository>();
        _matchRepositoryMock = new Mock<IMatchRepository>();
        var _mockLogger = new Mock<ILogger<DeletePlayerUseCase>>();

        _useCase = new DeletePlayerUseCase(
            _mockRepository.Object,
            _matchRepositoryMock.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPlayerDoesNotExist_ReturnsFailure()
    {
        var playerId = Guid.NewGuid();

        var request = new DeletePlayerRequest(playerId);

        _mockRepository.Setup(repo => repo.GetByIdAsync(playerId))
            .ReturnsAsync((Domain.Entities.Player?)null);

        var result = await _useCase.ExecuteAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("Гравця не знайдено.", result.ErrorMessage);
        _mockRepository.Verify(repo => repo.DeleteAsync(It.IsAny<Domain.Entities.Player>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPlayerExists_CallsRepositoryAndReturnsSuccess()
    {
        var player = new Domain.Entities.Player("Marharyta");
        var request = new DeletePlayerRequest(player.Id);

        _mockRepository.Setup(repo => repo.GetByIdAsync(player.Id))
            .ReturnsAsync(player);
        _matchRepositoryMock.Setup(repository => repository.HasMatchesByPlayerIdAsync(player.Id))
            .ReturnsAsync(false);

        var result = await _useCase.ExecuteAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Null(result.ErrorMessage);
        _mockRepository.Verify(repo => repo.DeleteAsync(player), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPlayerParticipatedInMatches_MarksPlayerAsDeleted()
    {
        var player = new Domain.Entities.Player("Marharyta");
        var request = new DeletePlayerRequest(player.Id);

        _mockRepository.Setup(repo => repo.GetByIdAsync(player.Id))
            .ReturnsAsync(player);
        _matchRepositoryMock.Setup(repository => repository.HasMatchesByPlayerIdAsync(player.Id))
            .ReturnsAsync(true);

        var result = await _useCase.ExecuteAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal("<видалений>", player.Name);
        Assert.True(player.IsDeleted);
        _mockRepository.Verify(repo => repo.UpdateAsync(player), Times.Once);
        _mockRepository.Verify(repo => repo.DeleteAsync(It.IsAny<Domain.Entities.Player>()), Times.Never);
    }
}
