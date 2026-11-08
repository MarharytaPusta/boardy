using Boardy.Application.GameCatalog.ScoreTemplate;
using Boardy.Domain.Enums;
using System;
using System.Collections.Generic;

namespace Boardy.Application.GameCatalog.Game;

public record UpdateGameRequest(
    Guid GameId,
    string Name,
    string Description,
    int MinPlayers,
    int MaxPlayers,
    GameStatus Status
);