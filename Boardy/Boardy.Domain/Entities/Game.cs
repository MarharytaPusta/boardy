using Boardy.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boardy.Domain.Entities;

public class Game
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public int MinPlayers { get; private set; }
    public int MaxPlayers { get; private set; }
    public GameStatus Status { get; private set; }

    private readonly List<ScoreTemplate> _scoreTemplates = new();
    public IReadOnlyCollection<ScoreTemplate> ScoreTemplates => _scoreTemplates.AsReadOnly();

    public Game(string name, string description, int minPlayers, int maxPlayers, GameStatus status)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        MinPlayers = minPlayers;
        MaxPlayers = maxPlayers;
        Status = status;
    }

    public void AddScoreTemplate(ScoreTemplate template)
    {
        _scoreTemplates.Add(template);
    }

}
