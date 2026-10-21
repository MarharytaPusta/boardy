using Boardy.Application.GameCatalog.ScoreTemplate;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Boardy.Application.Tests.GameCatalog.ScoreTemplate
{
    public class CreateScoreTemplateUseCaseTests
    {
        private readonly Mock<IScoreTemplateRepository> _mockRepository;
        private readonly Mock<ILogger<CreateScoreTemplateUseCase>> _mockLogger;
        private readonly CreateScoreTemplateUseCase _useCase;

        public CreateScoreTemplateUseCaseTests()
        {
            _mockRepository = new Mock<IScoreTemplateRepository>();
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
            var request = new CreateScoreTemplateRequest(invalidName, criteria);

            var result = await _useCase.ExecuteAsync(request);

            Assert.False(result.IsSuccess);
            Assert.Equal("Назва шаблону не може бути порожньою", result.ErrorMessage);

            _mockRepository.Verify(repo => repo.AddAsync(It.IsAny<Domain.Entities.ScoreTemplate>()), Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_WithEmptyCriteriaList_ReturnsFailure()
        {
            var request = new CreateScoreTemplateRequest("Базовий шаблон", new List<CriterionDto>());

            var result = await _useCase.ExecuteAsync(request);

            Assert.False(result.IsSuccess);
            Assert.Equal("Додайте хоча б один критерій оцінювання", result.ErrorMessage);
            _mockRepository.Verify(repo => repo.AddAsync(It.IsAny<Domain.Entities.ScoreTemplate>()), Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_WithEmptyCriterionName_ReturnsFailure()
        {
            var criteria = new List<CriterionDto>
            {
                new CriterionDto("Правило 1", false),
                new CriterionDto("   ", true)
            };
            var request = new CreateScoreTemplateRequest("Базовий шаблон", criteria);

            var result = await _useCase.ExecuteAsync(request);

            Assert.False(result.IsSuccess);
            Assert.Equal("Критерій не може бути порожнім", result.ErrorMessage);
            _mockRepository.Verify(repo => repo.AddAsync(It.IsAny<Domain.Entities.ScoreTemplate>()), Times.Never);
        }

        [Fact]
        public async Task ExecuteAsync_WithValidRequest_SavesTemplateAndReturnsSuccess()
        {
            var criteria = new List<CriterionDto>
            {
                new CriterionDto("Перемога", false),
                new CriterionDto("Штраф", true)
            };
            var request = new CreateScoreTemplateRequest("Мій ідеальний шаблон", criteria);

            var result = await _useCase.ExecuteAsync(request);

            Assert.True(result.IsSuccess);

            _mockRepository.Verify(repo => repo.AddAsync(
                It.Is<Domain.Entities.ScoreTemplate>(t =>
                    t.Name == "Мій ідеальний шаблон" &&
                    t.Criteria.Count == 2)),
                Times.Once);
        }
    }
}