using System;

namespace Boardy.Domain.Entities
{
    public class Player
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public Player(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Player's name cannot be empty", nameof(name));

            Id = Guid.NewGuid();
            Name = name.Trim();
        }

        private Player()
        {
            Name = string.Empty;
        }
    }
}
