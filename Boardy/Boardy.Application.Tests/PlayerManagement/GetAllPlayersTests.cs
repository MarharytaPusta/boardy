using Boardy.Application.Player;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Boardy.Application.Tests.Authentication.Player
{
    public class GetAllPlayersTests
    {
        [Fact]
        public async Task ExecuteAsync_ReturnsPlayersFromRepository()
        {
            var mockRepository = new Mock<IPlayerRepository>();
            var mockLogger = new Mock<ILogger<GetAllPlayers>>();
            var players = new[]
            {
                new Domain.Entities.Player("Marharyta"),
                new Domain.Entities.Player("Danylo")
            };
            mockRepository.Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(players);
            var getAllPlayers = new GetAllPlayers(mockRepository.Object, mockLogger.Object);

            var result = await getAllPlayers.ExecuteAsync();

            Assert.True(result.IsSuccess);
            Assert.Same(players, result.Data);
            mockRepository.Verify(repo => repo.GetAllAsync(), Times.Once);
        }
    }
}
