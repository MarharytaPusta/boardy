using Boardy.Application.GameCatalog.Game;
using Boardy.Application.GameCatalog.ScoreTemplate;
using Boardy.Application.Player;
using Boardy.Application.Scoring;
using Boardy.Application.Statistics;
using Boardy.Domain.Entities;
using Boardy.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using DomainMatch = Boardy.Domain.Entities.Match;
using DomainPlayer = Boardy.Domain.Entities.Player;

namespace Boardy.Application.Tests.Scoring.UpdateMatchResults;

public class UpdateMatchResultsUseCaseTests
{
    private readonly Mock<IMatchRepository> _matchRepositoryMock = new();
    private readonly Mock<IScoreTemplateRepository> _scoreTemplateRepositoryMock = new();
    private readonly ScoreTemplate _scoreTemplate;
    private readonly UpdateMatchResultsUseCase _useCase;

    public UpdateMatchResultsUseCaseTests()
    {
        _scoreTemplate = new ScoreTemplate("Базовий");
        _scoreTemplate.AddCriterion(new ScoreCriterion("Очки"));
        _scoreTemplateRepositoryMock
            .Setup(repository => repository.GetByIdAsync(_scoreTemplate.Id))
            .ReturnsAsync(_scoreTemplate);
        var loggerMock = new Mock<ILogger<UpdateMatchResultsUseCase>>();
        _useCase = new UpdateMatchResultsUseCase(
            _matchRepositoryMock.Object,
            _scoreTemplateRepositoryMock.Object,
            loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidResults_UpdatesScoresAndWinners()
    {
        var firstPlayer = new DomainPlayer("Маргарита");
        var secondPlayer = new DomainPlayer("Максим");
        var match = CreateCompletedMatch(firstPlayer.Id, secondPlayer.Id, firstPlayer.Id);
        var participants = match.Participants.ToList();
        var request = CreateRequest(match, 10, 20);

        _matchRepositoryMock.Setup(repository => repository.GetByIdAsync(match.Id)).ReturnsAsync(match);

        var result = await _useCase.ExecuteAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal(10, participants[0].TotalScore);
        Assert.Equal(20, participants[1].TotalScore);
        Assert.False(participants[0].IsWinner);
        Assert.True(participants[1].IsWinner);
        _matchRepositoryMock.Verify(repository => repository.UpdateAsync(match), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenMatchIsInProgress_ReturnsFailure()
    {
        var match = new DomainMatch(_scoreTemplate.Id);
        match.AddParticipant(new MatchParticipant(Guid.NewGuid()));
        var request = CreateRequest(match, 10);

        _matchRepositoryMock.Setup(repository => repository.GetByIdAsync(match.Id)).ReturnsAsync(match);

        var result = await _useCase.ExecuteAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("Редагувати можна лише завершену партію.", result.ErrorMessage);
        _matchRepositoryMock.Verify(repository => repository.UpdateAsync(It.IsAny<DomainMatch>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenParticipantDoesNotBelongToMatch_ReturnsFailure()
    {
        var firstPlayer = new DomainPlayer("Маргарита");
        var secondPlayer = new DomainPlayer("Максим");
        var match = CreateCompletedMatch(firstPlayer.Id, secondPlayer.Id, firstPlayer.Id);
        var request = new UpdateMatchResultsRequest(
            match.Id,
            new List<MatchParticipantScoreRequest>
            {
                new(Guid.NewGuid(), new List<CriterionScoreRequest> { new(_scoreTemplate.Criteria.First().Id, 10) }),
                new(match.Participants.Last().Id, new List<CriterionScoreRequest> { new(_scoreTemplate.Criteria.First().Id, 20) })
            });

        _matchRepositoryMock.Setup(repository => repository.GetByIdAsync(match.Id)).ReturnsAsync(match);

        var result = await _useCase.ExecuteAsync(request);

        Assert.False(result.IsSuccess);
        Assert.Equal("Учасника не знайдено в цій партії.", result.ErrorMessage);
        _matchRepositoryMock.Verify(repository => repository.UpdateAsync(It.IsAny<DomainMatch>()), Times.Never);
    }

    private DomainMatch CreateCompletedMatch(Guid firstPlayerId, Guid secondPlayerId, Guid winnerPlayerId)
    {
        var match = new DomainMatch(_scoreTemplate.Id);
        var firstParticipant = new MatchParticipant(firstPlayerId);
        var secondParticipant = new MatchParticipant(secondPlayerId);
        match.AddParticipant(firstParticipant);
        match.AddParticipant(secondParticipant);
        match.SetWinners(match.Participants
            .Where(participant => participant.PlayerId == winnerPlayerId)
            .Select(participant => participant.Id));
        match.ChangeStatus(MatchStatus.Completed);

        return match;
    }

    private UpdateMatchResultsRequest CreateRequest(DomainMatch match, params int[] scores)
    {
        return new UpdateMatchResultsRequest(
            match.Id,
            match.Participants
                .Select((participant, index) => new MatchParticipantScoreRequest(
                    participant.Id,
                    new List<CriterionScoreRequest>
                    {
                        new(_scoreTemplate.Criteria.First().Id, scores[index])
                    }))
                .ToList());
    }
}
