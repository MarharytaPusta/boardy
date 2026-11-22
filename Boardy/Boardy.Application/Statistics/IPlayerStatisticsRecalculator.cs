using Boardy.Domain.Shared;

namespace Boardy.Application.Statistics;

public interface IPlayerStatisticsRecalculator
{
    Task<Result> RecalculateAsync(IEnumerable<Guid> playerIds);
}
