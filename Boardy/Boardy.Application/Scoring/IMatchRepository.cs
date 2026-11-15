using Boardy.Domain.Entities;

namespace Boardy.Application.Scoring;

public interface IMatchRepository
{
    Task AddAsync(Match match);
    Task<Match?> GetByIdAsync(Guid id);
    Task<IEnumerable<Match>> GetCompletedAsync();
    Task<IEnumerable<Match>> GetCompletedByPlayerIdsAsync(IEnumerable<Guid> playerIds);
    Task UpdateAsync(Match match);
}
