using System;

namespace Boardy.Domain.Entities
{
    public class Player
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public int PlayedGamesCount { get; private set; }

        public int PlayedMatchesCount { get; private set; }

        public int WinsCount { get; private set; }

        public Player(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Player's name cannot be empty", nameof(name));

            Id = Guid.NewGuid();
            Name = name.Trim();
        }

        public void UpdateStatistics(int playedGamesCount, int playedMatchesCount, int winsCount)
        {
            if (playedGamesCount < 0)
                throw new ArgumentOutOfRangeException(nameof(playedGamesCount));

            if (playedMatchesCount < 0)
                throw new ArgumentOutOfRangeException(nameof(playedMatchesCount));

            if (winsCount < 0 || winsCount > playedMatchesCount)
                throw new ArgumentOutOfRangeException(nameof(winsCount));

            PlayedGamesCount = playedGamesCount;
            PlayedMatchesCount = playedMatchesCount;
            WinsCount = winsCount;
        }

        private Player()
        {
            Name = string.Empty;
        }
    }
}
