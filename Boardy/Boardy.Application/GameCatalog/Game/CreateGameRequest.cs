using Boardy.Application.GameCatalog.ScoreTemplate;
using Boardy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boardy.Application.GameCatalog.Game;

public record CreateGameRequest(
    string Name,
    string Description,
    int MinPlayers,
    int MaxPlayers,
    GameStatus Status,
    List<CriterionDto> Criteria
);
