using Boardy.Application.Player;
using Boardy.Application.Scoring;
using Boardy.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using DomainMatch = Boardy.Domain.Entities.Match;
using DomainPlayer = Boardy.Domain.Entities.Player;

namespace Boardy.Application.Tests.Scoring.GetMatchResults;

public class GetMatchResultsUseCaseTests
{
    private readonly Mock<IMatchRepository> _matchRepositoryMock;
    private readonly Mock<IPlayerRepository> _playerRepositoryMock;
    private readonly GetMatchResultsUseCase _useCase;

    public GetMatchResultsUseCaseTests()
    {
        _matchRepositoryMock = new Mock<IMatchRepository>();
        _playerRepositoryMock = new Mock<IPlayerRepository>();
        var loggerMock = new Mock<ILogger<GetMatchResultsUseCase>>();

        _useCase = new GetMatchResultsUseCase(
            _matchRepositoryMock.Object,
            _playerRepositoryMock.Object,
            loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidMatch_ReturnsParticipantResults()
    {
        var firstPlayer = new DomainPlayer("Олена");
        var secondPlayer = new DomainPlayer("Тарас");
        var match = CreateMatch(firstPlayer.Id, secondPlayer.Id);

        match.UpdateParticipantScore(match.Participants.First().Id, 20);
        match.UpdateParticipantScore(match.Participants.Last().Id, 15);

        _matchRepositoryMock.Setup(repository => repository.GetByIdAsync(match.Id)).ReturnsAsync(match);
        _playerRepositoryMock
            .Setup(repository => repository.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(new List<DomainPlayer> { firstPlayer, secondPlayer });

        var result = await _useCase.ExecuteAsync(new GetMatchResultsRequest(match.Id));

        Assert.True(result.IsSuccess);
        var data = Assert.IsType<MatchResultsDto>(result.Data);
        Assert.Equal(match.Id, data.Id);
        Assert.Equal(2, data.Participants.Count());
        Assert.Contains(data.Participants, participant =>
            participant.PlayerName == "Олена" && participant.TotalScore == 20);
        Assert.Contains(data.Participants, participant =>
            participant.PlayerName == "Тарас" && participant.TotalScore == 15);
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyMatchId_ReturnsFailure()
    {
        var result = await _useCase.ExecuteAsync(new GetMatchResultsRequest(Guid.Empty));

        Assert.False(result.IsSuccess);
        Assert.Equal("Не вказано ідентифікатор партії.", result.ErrorMessage);
        _matchRepositoryMock.Verify(repository => repository.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenMatchDoesNotExist_ReturnsFailure()
    {
        var matchId = Guid.NewGuid();
        _matchRepositoryMock.Setup(repository => repository.GetByIdAsync(matchId)).ReturnsAsync((DomainMatch?)null);

        var result = await _useCase.ExecuteAsync(new GetMatchResultsRequest(matchId));

        Assert.False(result.IsSuccess);
        Assert.Equal("Партію не знайдено в базі даних.", result.ErrorMessage);
        _playerRepositoryMock.Verify(repository => repository.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>()), Times.Never);
    }

    private static DomainMatch CreateMatch(Guid firstPlayerId, Guid secondPlayerId)
    {
        var match = new DomainMatch(Guid.NewGuid());
        match.AddParticipant(new MatchParticipant(firstPlayerId));
        match.AddParticipant(new MatchParticipant(secondPlayerId));

        return match;
    }
}
