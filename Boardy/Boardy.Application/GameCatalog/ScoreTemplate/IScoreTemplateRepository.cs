using System.Threading.Tasks;

namespace Boardy.Application.GameCatalog.ScoreTemplate
{
    public interface IScoreTemplateRepository
    {
        Task AddAsync(Domain.Entities.ScoreTemplate template);
    }
}