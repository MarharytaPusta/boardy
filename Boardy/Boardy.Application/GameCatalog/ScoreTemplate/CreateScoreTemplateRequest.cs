using System.Collections.Generic;

namespace Boardy.Application.GameCatalog.ScoreTemplate
{
    public record CriterionDto(string Name, bool IsNegative);
    public record CreateScoreTemplateRequest(Guid GameId, string TemplateName, List<CriterionDto> Criteria);
}