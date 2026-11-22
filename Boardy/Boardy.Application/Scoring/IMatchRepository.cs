using Boardy.Domain.Entities;

namespace Boardy.Application.Scoring;

public interface IMatchRepository
{
    Task AddAsync(Match match);
    Task<Match?> GetByIdAsync(Guid id);
    Task<IEnumerable<Match>> GetCompletedAsync();
    Task<IEnumerable<Match>> GetCompletedByPlayerIdsAsync(IEnumerable<Guid> playerIds);
    Task<bool> HasMatchesByPlayerIdAsync(Guid playerId);
    Task UpdateAsync(Match match);
    Task DeleteAsync(Match match);

    Task<IReadOnlyDictionary<Guid, int>> GetMatchesCountByGameAsync();
}
