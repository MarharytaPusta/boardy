using Boardy.Application.GameCatalog.Statistics;
using Boardy.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Boardy.Infrastructure.Repositories;

public class GameStatisticsRepository : IGameStatisticsRepository
{
    private readonly BoardyDbContext _context;

    public GameStatisticsRepository(BoardyDbContext context)
    {
        _context = context;
    }

    public async Task<GameStatisticsDto?> GetByGameIdAsync(
        Guid gameId,
        int topResultsLimit = 3,
        CancellationToken cancellationToken = default)
    {
        var gameInfo = await _context.Games
            .AsNoTracking()
            .Where(g => g.Id == gameId)
            .Select(g => new { g.Id, g.Name })
            .FirstOrDefaultAsync(cancellationToken);

        if (gameInfo is null)
        {
            return null;
        }

        var gameTemplates = await _context.Games
            .AsNoTracking()
            .Where(g => g.Id == gameId)
            .SelectMany(g => g.ScoreTemplates)
            .Select(st => new { st.Id, st.Name })
            .ToListAsync(cancellationToken);

        if (gameTemplates.Count == 0)
        {
            return new GameStatisticsDto(
                gameInfo.Id,
                gameInfo.Name,
                TotalPlayedMatches: 0,
                TopResults: Array.Empty<TopScoreDto>(),
                Players: Array.Empty<GamePlayerStatDto>());
        }

        var templateNamesById = gameTemplates.ToDictionary(t => t.Id, t => t.Name);
        var templateIds = templateNamesById.Keys.ToList();

        var completedMatchesQuery = _context.Matches
            .AsNoTracking()
            .Where(m => templateIds.Contains(m.ScoreTemplateId) && m.Status == MatchStatus.Completed);

        var totalPlayedMatches = await completedMatchesQuery.CountAsync(cancellationToken);

        if (totalPlayedMatches == 0)
        {
            return new GameStatisticsDto(
                gameInfo.Id,
                gameInfo.Name,
                TotalPlayedMatches: 0,
                TopResults: Array.Empty<TopScoreDto>(),
                Players: Array.Empty<GamePlayerStatDto>());
        }

        var participantRows = await (
            from match in completedMatchesQuery
            from participant in match.Participants
            join player in _context.Players.AsNoTracking() on participant.PlayerId equals player.Id
            select new
            {
                PlayerId = player.Id,
                PlayerName = player.Name,
                IsPlayerDeleted = player.IsDeleted,
                participant.TotalScore,
                participant.IsWinner,
                match.Date,
                match.ScoreTemplateId
            })
            .ToListAsync(cancellationToken);

        var topResults = participantRows
            .OrderByDescending(x => x.TotalScore)
            .ThenByDescending(x => x.Date)
            .DistinctBy(x => x.PlayerId)
            .Take(topResultsLimit)
            .Select(x => new TopScoreDto(
                x.PlayerId,
                x.PlayerName,
                x.IsPlayerDeleted,
                x.TotalScore,
                x.Date,
                templateNamesById.GetValueOrDefault(x.ScoreTemplateId, string.Empty)))
            .ToList();

        var players = participantRows
            .GroupBy(x => new { x.PlayerId, x.PlayerName, x.IsPlayerDeleted })
            .Select(g => new GamePlayerStatDto(
                g.Key.PlayerId,
                g.Key.PlayerName,
                g.Key.IsPlayerDeleted,
                MatchesPlayedInGame: g.Count(),
                WinsInGame: g.Count(x => x.IsWinner),
                BestScoreInGame: g.Max(x => x.TotalScore)))
            .OrderByDescending(p => p.WinsInGame)
            .ThenByDescending(p => p.MatchesPlayedInGame)
            .ToList();

        return new GameStatisticsDto(
            gameInfo.Id,
            gameInfo.Name,
            totalPlayedMatches,
            topResults,
            players);
    }
}