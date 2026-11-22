using Boardy.Application.GameCatalog.Game;
using Boardy.Application.Player;
using Boardy.Application.Scoring;
using Boardy.Application.Statistics;
using Boardy.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using DomainMatch = Boardy.Domain.Entities.Match;
using DomainPlayer = Boardy.Domain.Entities.Player;

namespace Boardy.Application.Tests.Statistics;

public class GetPlayerStatisticsUseCaseTests
{
    private readonly Mock<IPlayerRepository> _playerRepositoryMock = new();
    private readonly Mock<IMatchRepository> _matchRepositoryMock = new();
    private readonly Mock<IGameRepository> _gameRepositoryMock = new();
    private readonly GetPlayerStatisticsUseCase _useCase;

    public GetPlayerStatisticsUseCaseTests()
    {
        var loggerMock = new Mock<ILogger<GetPlayerStatisticsUseCase>>();
        _useCase = new GetPlayerStatisticsUseCase(
            _playerRepositoryMock.Object,
            _matchRepositoryMock.Object,
            _gameRepositoryMock.Object,
            loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsPlayerAndGameStatistics()
    {
        var player = new DomainPlayer("Олена");
        player.UpdateStatistics(2, 3, 2);
        var firstGameId = Guid.NewGuid();
        var secondGameId = Guid.NewGuid();
        var firstGameTemplateId = Guid.NewGuid();
        var secondTemplateForFirstGameId = Guid.NewGuid();
        var secondGameTemplateId = Guid.NewGuid();
        var firstMatch = CreateCompletedMatch(firstGameTemplateId, player.Id, isPlayerWinner: true);
        var secondMatch = CreateCompletedMatch(secondTemplateForFirstGameId, player.Id, isPlayerWinner: false);
        var thirdMatch = CreateCompletedMatch(secondGameTemplateId, player.Id, isPlayerWinner: true);
        var firstGame = new Domain.Entities.Game("Крила", string.Empty, 2, 4, GameStatus.Active);
        var secondGame = new Domain.Entities.Game("Каркасон", string.Empty, 2, 4, GameStatus.Active);

        _playerRepositoryMock.Setup(repository => repository.GetByIdAsync(player.Id))
            .ReturnsAsync(player);
        _matchRepositoryMock.Setup(repository => repository.GetCompletedByPlayerIdsAsync(
                It.Is<IEnumerable<Guid>>(ids => ids.Single() == player.Id)))
            .ReturnsAsync(new[] { firstMatch, secondMatch, thirdMatch });
        _gameRepositoryMock.Setup(repository => repository.GetGameIdsByScoreTemplateIdsAsync(
                It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(new Dictionary<Guid, Guid>
            {
                [firstGameTemplateId] = firstGameId,
                [secondTemplateForFirstGameId] = firstGameId,
                [secondGameTemplateId] = secondGameId
            });
        _gameRepositoryMock.Setup(repository => repository.GetByIdAsync(firstGameId))
            .ReturnsAsync(firstGame);
        _gameRepositoryMock.Setup(repository => repository.GetByIdAsync(secondGameId))
            .ReturnsAsync(secondGame);

        var result = await _useCase.ExecuteAsync(player.Id);

        Assert.True(result.IsSuccess);
        var statistics = result.Data!;
        Assert.Equal(player.Id, statistics.PlayerId);
        Assert.Equal("Олена", statistics.PlayerName);
        Assert.Equal(2, statistics.PlayedGamesCount);
        Assert.Equal(3, statistics.PlayedMatchesCount);
        Assert.Equal(2, statistics.WinsCount);
        Assert.Collection(
            statistics.Games,
            game =>
            {
                Assert.Equal("Каркасон", game.GameName);
                Assert.Equal(1, game.PlayedMatchesCount);
                Assert.Equal(1, game.WinsCount);
            },
            game =>
            {
                Assert.Equal("Крила", game.GameName);
                Assert.Equal(2, game.PlayedMatchesCount);
                Assert.Equal(1, game.WinsCount);
            });
    }

    [Fact]
    public async Task ExecuteAsync_WhenPlayerDoesNotExist_ReturnsFailure()
    {
        var playerId = Guid.NewGuid();
        _playerRepositoryMock.Setup(repository => repository.GetByIdAsync(playerId))
            .ReturnsAsync((DomainPlayer?)null);

        var result = await _useCase.ExecuteAsync(playerId);

        Assert.False(result.IsSuccess);
        Assert.Equal("Гравця не знайдено.", result.ErrorMessage);
        _matchRepositoryMock.Verify(
            repository => repository.GetCompletedByPlayerIdsAsync(It.IsAny<IEnumerable<Guid>>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithNoPlayedMatches_ReturnsEmptyGames()
    {
        var player = new DomainPlayer("Тарас");
        _playerRepositoryMock.Setup(repository => repository.GetByIdAsync(player.Id))
            .ReturnsAsync(player);
        _matchRepositoryMock.Setup(repository => repository.GetCompletedByPlayerIdsAsync(
                It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(Array.Empty<DomainMatch>());

        var result = await _useCase.ExecuteAsync(player.Id);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Data!.Games);
        _gameRepositoryMock.Verify(
            repository => repository.GetGameIdsByScoreTemplateIdsAsync(It.IsAny<IEnumerable<Guid>>()),
            Times.Never);
    }

    private static DomainMatch CreateCompletedMatch(Guid scoreTemplateId, Guid playerId, bool isPlayerWinner)
    {
        var match = new DomainMatch(scoreTemplateId);
        var playerParticipant = new Domain.Entities.MatchParticipant(playerId);
        var anotherPlayerParticipant = new Domain.Entities.MatchParticipant(Guid.NewGuid());
        match.AddParticipant(playerParticipant);
        match.AddParticipant(anotherPlayerParticipant);

        if (isPlayerWinner)
        {
            match.SetWinners(new[] { playerParticipant.Id });
        }
        else
        {
            match.SetWinners(new[] { anotherPlayerParticipant.Id });
        }

        match.ChangeStatus(MatchStatus.Completed);
        return match;
    }
}
