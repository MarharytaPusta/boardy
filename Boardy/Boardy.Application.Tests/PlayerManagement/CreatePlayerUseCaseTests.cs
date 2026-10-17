using Boardy.Application.Player;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Boardy.Application.Tests.Authentication.Player
{
    public class CreatePlayerUseCaseTests
    {
        private readonly Mock<IPlayerRepository> _mockRepository;
        private readonly CreatePlayerUseCase _useCase;

        public CreatePlayerUseCaseTests()
        {
            _mockRepository = new Mock<IPlayerRepository>();
            var _mockLogger = new Mock<ILogger<CreatePlayerUseCase>>();

            _useCase = new CreatePlayerUseCase(_mockRepository.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task ExecuteAsync_WithValidRequest_SavesPlayerToRepository()
        {
            var expectedName = "Marharyta";
            var request = new CreatePlayerRequest(expectedName);

            await _useCase.ExecuteAsync(request);

            _mockRepository.Verify(repo => repo.AddAsync(
                It.Is<Domain.Entities.Player>(p => p.Name == expectedName)),
                Times.Once);
        }
    }
}