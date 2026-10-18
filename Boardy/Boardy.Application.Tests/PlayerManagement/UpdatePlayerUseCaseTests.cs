using Boardy.Application.Player;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Boardy.Application.Tests.Authentication.Player
{
    public class UpdatePlayerUseCaseTests
    {
        private readonly Mock<IPlayerRepository> _mockRepository;
        private readonly UpdatePlayerUseCase _useCase;

        public UpdatePlayerUseCaseTests()
        {
            _mockRepository = new Mock<IPlayerRepository>();
            var _mockLogger = new Mock<ILogger<UpdatePlayerUseCase>>();

            _useCase = new UpdatePlayerUseCase(_mockRepository.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task ExecuteAsync_WithValidRequest_UpdatesPlayer()
        {
            var player = new Domain.Entities.Player("Marharyta");
            var request = new UpdatePlayerRequest(player.Id, " Danylo ");
            _mockRepository.Setup(repo => repo.GetByIdAsync(player.Id))
                .ReturnsAsync(player);

            var result = await _useCase.ExecuteAsync(request);

            Assert.True(result.IsSuccess);
            Assert.Equal("Danylo", player.Name);
            _mockRepository.Verify(repo => repo.UpdateAsync(player), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_WithEmptyName_ReturnsFailure()
        {
            var request = new UpdatePlayerRequest(Guid.NewGuid(), "   ");

            var result = await _useCase.ExecuteAsync(request);

            Assert.False(result.IsSuccess);
            Assert.Equal("Ім'я не може бути порожнім.", result.ErrorMessage);
            _mockRepository.Verify(repo => repo.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
            _mockRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Domain.Entities.Player>()), Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_WhenPlayerDoesNotExist_ReturnsFailure()
        {
            var playerId = Guid.NewGuid();
            var request = new UpdatePlayerRequest(playerId, "Danylo");
            _mockRepository.Setup(repo => repo.GetByIdAsync(playerId))
                .ReturnsAsync((Domain.Entities.Player?)null);

            var result = await _useCase.ExecuteAsync(request);

            Assert.False(result.IsSuccess);
            Assert.Equal("Гравця не знайдено.", result.ErrorMessage);
            _mockRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Domain.Entities.Player>()), Times.Never);
        }
    }
}
