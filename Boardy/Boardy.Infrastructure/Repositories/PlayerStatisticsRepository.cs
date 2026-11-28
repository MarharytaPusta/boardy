using Boardy.Application.Statistics;
using Boardy.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Boardy.Infrastructure.Repositories;

public class PlayerStatisticsRepository : IPlayerStatisticsRepository
{
    private readonly BoardyDbContext _context;

    public PlayerStatisticsRepository(BoardyDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<GetOverallRatingDto>> GetOverallRatingAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Players
            .AsNoTracking()
            .Where(player => !player.IsDeleted)
            .Select(player => new
            {
                player.Id,
                player.Name,
                PlayedGamesCount = _context.Games.Count(game =>
                    game.ScoreTemplates.Any(template =>
                        _context.Matches.Any(match =>
                            match.Status == MatchStatus.Completed &&
                            match.ScoreTemplateId == template.Id &&
                            match.Participants.Any(participant => participant.PlayerId == player.Id)))),
                PlayedMatchesCount = _context.Matches.Count(match =>
                    match.Status == MatchStatus.Completed &&
                    match.Participants.Any(participant => participant.PlayerId == player.Id)),
                WinsCount = _context.Matches.Count(match =>
                    match.Status == MatchStatus.Completed &&
                    match.Participants.Any(participant => participant.PlayerId == player.Id && participant.IsWinner))
            })
            .OrderByDescending(x => x.WinsCount)
            .ThenByDescending(x => x.PlayedMatchesCount)
            .ThenByDescending(x => x.PlayedGamesCount)
            .ThenBy(x => x.Name)
            .Select(x => new GetOverallRatingDto(
                x.Id,
                x.Name,
                x.PlayedGamesCount,
                x.PlayedMatchesCount,
                x.WinsCount))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PlayerGameStatisticsDto>> GetPlayerGamesStatisticsAsync(
            Guid playerId,
            CancellationToken cancellationToken = default)
    {
        return await _context.Games
            .AsNoTracking()
            .Select(game => new
            {
                game.Id,
                game.Name,
                PlayedMatchesCount = _context.Matches.Count(match =>
                    match.Status == MatchStatus.Completed &&
                    game.ScoreTemplates.Any(template => template.Id == match.ScoreTemplateId) &&
                    match.Participants.Any(participant => participant.PlayerId == playerId)),
                WinsCount = _context.Matches.Count(match =>
                    match.Status == MatchStatus.Completed &&
                    game.ScoreTemplates.Any(template => template.Id == match.ScoreTemplateId) &&
                    match.Participants.Any(participant => participant.PlayerId == playerId && participant.IsWinner))
            })
            .Where(x => x.PlayedMatchesCount > 0)
            .OrderBy(x => x.Name)
            .Select(x => new PlayerGameStatisticsDto(
                x.Name,
                x.PlayedMatchesCount,
                x.WinsCount))
            .ToListAsync(cancellationToken);
    }
}