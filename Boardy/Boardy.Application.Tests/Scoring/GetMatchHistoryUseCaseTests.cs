using Boardy.Application.GameCatalog.Game;
using Boardy.Application.Player;
using Boardy.Application.Scoring;
using Boardy.Domain.Entities;
using Boardy.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using DomainMatch = Boardy.Domain.Entities.Match;
using DomainPlayer = Boardy.Domain.Entities.Player;

namespace Boardy.Application.Tests.Scoring.GetMatchHistory;

public class GetMatchHistoryUseCaseTests
{
    private readonly Mock<IMatchRepository> _matchRepositoryMock = new();
    private readonly Mock<IPlayerRepository> _playerRepositoryMock = new();
    private readonly Mock<IGameRepository> _gameRepositoryMock = new();
    private readonly GetMatchHistoryUseCase _useCase;

    public GetMatchHistoryUseCaseTests()
    {
        var loggerMock = new Mock<ILogger<GetMatchHistoryUseCase>>();
        _useCase = new GetMatchHistoryUseCase(
            _matchRepositoryMock.Object,
            _playerRepositoryMock.Object,
            _gameRepositoryMock.Object,
            loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithCompletedMatches_ReturnsHistoryItems()
    {
        var firstPlayer = new DomainPlayer("Олена");
        var secondPlayer = new DomainPlayer("Тарас");
        var scoreTemplateId = Guid.NewGuid();
        var match = CreateCompletedMatch(scoreTemplateId, firstPlayer.Id, secondPlayer.Id, secondPlayer.Id);

        _matchRepositoryMock.Setup(repository => repository.GetCompletedAsync())
            .ReturnsAsync(new List<DomainMatch> { match });
        _playerRepositoryMock.Setup(repository => repository.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(new List<DomainPlayer> { firstPlayer, secondPlayer });
        _gameRepositoryMock.Setup(repository => repository.GetNameByScoreTemplateIdAsync(scoreTemplateId))
            .ReturnsAsync("Крила");

        var result = await _useCase.ExecuteAsync();

        Assert.True(result.IsSuccess);
        var historyItem = Assert.Single(result.Data!);
        Assert.Equal(match.Id, historyItem.Id);
        Assert.Equal("Крила", historyItem.GameName);
        Assert.Equal("Олена, Тарас", historyItem.Participants);
        Assert.Equal("Тарас", historyItem.Winners);
    }

    [Fact]
    public async Task ExecuteAsync_WithNoCompletedMatches_ReturnsEmptyHistory()
    {
        _matchRepositoryMock.Setup(repository => repository.GetCompletedAsync())
            .ReturnsAsync(new List<DomainMatch>());
        _playerRepositoryMock.Setup(repository => repository.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(new List<DomainPlayer>());

        var result = await _useCase.ExecuteAsync();

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Data!);
        _gameRepositoryMock.Verify(
            repository => repository.GetNameByScoreTemplateIdAsync(It.IsAny<Guid>()),
            Times.Never);
    }

    private static DomainMatch CreateCompletedMatch(
        Guid scoreTemplateId,
        Guid firstPlayerId,
        Guid secondPlayerId,
        Guid winnerPlayerId)
    {
        var match = new DomainMatch(scoreTemplateId);
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
}
