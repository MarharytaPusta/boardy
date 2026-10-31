using Boardy.Domain.Entities;

namespace Boardy.Application.Scoring;

public interface IMatchRepository
{
    Task AddAsync(Match match);
    Task<Match?> GetByIdAsync(Guid id);
}
