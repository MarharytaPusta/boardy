using Boardy.Application.GameCatalog.ScoreTemplate;
using Boardy.Domain.Shared;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace Boardy.Application.GameCatalog.ScoreTemplate
{
    public class CreateScoreTemplateUseCase
    {
        private readonly IScoreTemplateRepository _repository;
        private readonly ILogger<CreateScoreTemplateUseCase> _logger;

        public CreateScoreTemplateUseCase(
            IScoreTemplateRepository repository,
            ILogger<CreateScoreTemplateUseCase> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<Result> ExecuteAsync(CreateScoreTemplateRequest request)
        {
            var validationResult = Validate(request);
            if (!validationResult.IsSuccess)
            {
                return validationResult;
            }

            var template = new Domain.Entities.ScoreTemplate(request.TemplateName);

            foreach (var criterionDto in request.Criteria)
            {
                var criterion = new Domain.Entities.ScoreCriterion(criterionDto.Name, criterionDto.IsNegative);
                template.AddCriterion(criterion);
            }

            await _repository.AddAsync(template);

            _logger.LogInformation("Successfully created ScoreTemplate '{TemplateName}' with {CriteriaCount} criteria (ID: {TemplateId}).",
                template.Name, template.Criteria.Count, template.Id);

            return Result.Success();
        }

        static private Result Validate(CreateScoreTemplateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.TemplateName))
            {
                return Result.Failure("Назва шаблону не може бути порожньою");
            }

            if (request.Criteria == null || request.Criteria.Count == 0)
            {
                return Result.Failure("Додайте хоча б один критерій оцінювання");
            }

            if (request.Criteria.Any(c => string.IsNullOrWhiteSpace(c.Name)))
            {
                return Result.Failure("Критерій не може бути порожнім");
            }

            bool hasDuplicates = request.Criteria
                .GroupBy(c => c.Name.Trim(), StringComparer.OrdinalIgnoreCase)
                .Any(g => g.Count() > 1);

            if (hasDuplicates)
            {
                return Result.Failure("Критерії в одному шаблоні не можуть мати однакові назви");
            }

            return Result.Success();
        }
    }
}