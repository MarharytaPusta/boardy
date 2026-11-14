using Boardy.Application.GameCatalog.Game;
using Boardy.Application.GameCatalog.ScoreTemplate;
using Boardy.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Boardy.Application.Tests.GameCatalog.ScoreTemplate
{
    public class CreateScoreTemplateUseCaseTests
    {
        private readonly Mock<IGameRepository> _mockRepository;
        private readonly Mock<ILogger<CreateScoreTemplateUseCase>> _mockLogger;
        private readonly CreateScoreTemplateUseCase _useCase;

        public CreateScoreTemplateUseCaseTests()
        {
            _mockRepository = new Mock<IGameRepository>();
            _mockLogger = new Mock<ILogger<CreateScoreTemplateUseCase>>();

            _useCase = new CreateScoreTemplateUseCase(_mockRepository.Object, _mockLogger.Object);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task ExecuteAsync_WithEmptyTemplateName_ReturnsFailure(string invalidName)
        {
            var criteria = new List<CriterionDto> { new CriterionDto("Правило 1", false) };
            var request = new CreateScoreTemplateRequest(Guid.NewGuid(), invalidName, criteria);

            var result = await _useCase.ExecuteAsync(request);

            Assert.False(result.IsSuccess);
            Assert.Equal("Назва шаблону не може бути порожньою", result.ErrorMessage);

            _mockRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Domain.Entities.Game>()), Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_WithEmptyCriteriaList_ReturnsFailure()
        {
            var request = new CreateScoreTemplateRequest(Guid.NewGuid(), "Базовий шаблон", new List<CriterionDto>());

            var result = await _useCase.ExecuteAsync(request);

            Assert.False(result.IsSuccess);
            Assert.Equal("Додайте хоча б один критерій оцінювання", result.ErrorMessage);
            _mockRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Domain.Entities.Game>()), Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_WithEmptyCriterionName_ReturnsFailure()
        {
            var criteria = new List<CriterionDto>
            {
                new CriterionDto("Правило 1", false),
                new CriterionDto("   ", true)
            };
            var request = new CreateScoreTemplateRequest(Guid.NewGuid(), "Базовий шаблон", criteria);

            var result = await _useCase.ExecuteAsync(request);

            Assert.False(result.IsSuccess);
            Assert.Equal("Критерій не може бути порожнім", result.ErrorMessage);
            _mockRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Domain.Entities.Game>()), Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_WithDuplicateCriteriaNames_ReturnsFailure()
        {
            var criteria = new List<CriterionDto>
            {
                new CriterionDto("Бали", false),
                new CriterionDto("бали ", true)
            };
            var request = new CreateScoreTemplateRequest(Guid.NewGuid(), "Базовий шаблон", criteria);

            var result = await _useCase.ExecuteAsync(request);

            Assert.False(result.IsSuccess);
            Assert.Equal("Критерії в одному шаблоні не можуть мати однакові назви", result.ErrorMessage);
            _mockRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Domain.Entities.Game>()), Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_WhenGameNotFound_ReturnsFailure()
        {
            var gameId = Guid.NewGuid();
            _mockRepository.Setup(repo => repo.GetByIdAsync(gameId)).ReturnsAsync((Domain.Entities.Game?)null);

            var criteria = new List<CriterionDto> { new CriterionDto("Перемога", false) };
            var request = new CreateScoreTemplateRequest(gameId, "Базовий шаблон", criteria);

            var result = await _useCase.ExecuteAsync(request);

            Assert.False(result.IsSuccess);
            _mockRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Domain.Entities.Game>()), Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_WithValidRequest_SavesTemplateAndReturnsSuccess()
        {
            var game = new Domain.Entities.Game("Крила", "Опис гри", 1, 5, GameStatus.Active);
            _mockRepository.Setup(repo => repo.GetByIdAsync(game.Id)).ReturnsAsync(game);

            var criteria = new List<CriterionDto>
            {
                new CriterionDto("Перемога", false),
                new CriterionDto("Штраф", true)
            };
            var request = new CreateScoreTemplateRequest(game.Id, "Мій ідеальний шаблон", criteria);

            var result = await _useCase.ExecuteAsync(request);

            Assert.True(result.IsSuccess);

            var addedTemplate = Assert.Single(game.ScoreTemplates);
            Assert.Equal("Мій ідеальний шаблон", addedTemplate.Name);
            Assert.Equal(2, addedTemplate.Criteria.Count);

            _mockRepository.Verify(repo => repo.UpdateAsync(game), Times.Once);
        }
    }
}