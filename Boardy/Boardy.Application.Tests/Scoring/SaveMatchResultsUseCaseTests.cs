using Boardy.Application.Scoring;
using Boardy.Domain.Entities;
using Boardy.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using DomainMatch = Boardy.Domain.Entities.Match;

namespace Boardy.Application.Tests.Scoring.SaveMatchResults;

public class SaveMatchResultsUseCaseTests
{
    private readonly Mock<IMatchRepository> _matchRepositoryMock;
    private readonly SaveMatchResultsUseCase _useCase;

    public SaveMatchResultsUseCaseTests()
    {
        _matchRepositoryMock = new Mock<IMatchRepository>();
        var loggerMock = new Mock<ILogger<SaveMatchResultsUseCase>>();

        _useCase = new SaveMatchResultsUseCase(_matchRepositoryMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidResults_SavesMatchAndSetsWinner()
    {
        var match = CreateMatch();
        var participants = match.Participants.ToList();
        var request = CreateRequest(match, 10, 20);

        _matchRepositoryMock.Setup(repository => repository.GetByIdAsync(match.Id)).ReturnsAsync(match);

        var result = await _useCase.ExecuteAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal(MatchStatus.Completed, match.Status);
        Assert.Equal(10, participants[0].TotalScore);
        Assert.Equal(20, participants[1].TotalScore);
        Assert.False(participants[0].IsWinner);
        Assert.True(participants[1].IsWinner);
        _matchRepositoryMock.Verify(repository => repository.UpdateAsync(match), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithTie_SetsAllTopParticipantsAsWinners()
    {
        var match = CreateMatch();
        var request = CreateRequest(match, 20, 20);

        _matchRepositoryMock.Setup(repository => repository.GetByIdAsync(match.Id)).ReturnsAsync(match);

        var result = await _useCase.ExecuteAsync(request);

        Assert.True(result.IsSuccess);
        Assert.All(match.Participants, participant => Assert.True(participant.IsWinner));
        _matchRepositoryMock.Verify(repository => repository.UpdateAsync(match), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyMatchId_ReturnsFailure()
    {
        var request = new SaveMatchResultsRequest(Guid.Empty, new List<MatchParticipantScoreRequest>());

        var result = await _useCase.ExecuteAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("Не вказано ідентифікатор партії.", result.ErrorMessage);
        _matchRepositoryMock.Verify(repository => repository.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        _matchRepositoryMock.Verify(repository => repository.UpdateAsync(It.IsAny<DomainMatch>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithDuplicateParticipantResults_ReturnsFailure()
    {
        var participantId = Guid.NewGuid();
        var request = new SaveMatchResultsRequest(
            Guid.NewGuid(),
            new List<MatchParticipantScoreRequest>
            {
                new(participantId, 10),
                new(participantId, 20)
            });

        var result = await _useCase.ExecuteAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("Результати одного учасника не можуть бути вказані двічі.", result.ErrorMessage);
        _matchRepositoryMock.Verify(repository => repository.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenMatchIsCompleted_ReturnsFailure()
    {
        var match = CreateMatch();
        match.ChangeStatus(MatchStatus.Completed);
        var request = CreateRequest(match, 10, 20);

        _matchRepositoryMock.Setup(repository => repository.GetByIdAsync(match.Id)).ReturnsAsync(match);

        var result = await _useCase.ExecuteAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("Результати цієї партії вже збережено.", result.ErrorMessage);
        _matchRepositoryMock.Verify(repository => repository.UpdateAsync(It.IsAny<DomainMatch>()), Times.Never);
    }

    private static DomainMatch CreateMatch()
    {
        var match = new DomainMatch(Guid.NewGuid());
        match.AddParticipant(new MatchParticipant(Guid.NewGuid()));
        match.AddParticipant(new MatchParticipant(Guid.NewGuid()));

        return match;
    }

    private static SaveMatchResultsRequest CreateRequest(DomainMatch match, int firstScore, int secondScore)
    {
        var participants = match.Participants.ToList();

        return new SaveMatchResultsRequest(
            match.Id,
            new List<MatchParticipantScoreRequest>
            {
                new(participants[0].Id, firstScore),
                new(participants[1].Id, secondScore)
            });
    }
}
