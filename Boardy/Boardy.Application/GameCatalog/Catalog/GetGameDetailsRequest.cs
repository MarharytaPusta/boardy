using Boardy.Application.GameCatalog.ScoreTemplate;
using Boardy.Domain.Enums;
using System;
using System.Collections.Generic;

namespace Boardy.Application.GameCatalog.Catalog;

public record GetGameDetailsRequest(Guid GameId);

public record GameDetailsDto(
    Guid Id,
    string Name,
    string Description,
    int MinPlayers,
    int MaxPlayers,
    GameStatus Status,
    IEnumerable<ScoreTemplateDto> Templates
);

public record ScoreTemplateDto(
    string Name,
    IEnumerable<CriterionDto> Criteria
);