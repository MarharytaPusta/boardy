using System;

namespace Boardy.Domain.Entities
{
    public class Player
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public bool IsDeleted { get; private set; }

        public Player(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Player's name cannot be empty", nameof(name));

            Id = Guid.NewGuid();
            Name = name.Trim();
        }

        public void MarkAsDeleted()
        {
            Name = "<видалений>";
            IsDeleted = true;
        }

        private Player()
        {
            Name = string.Empty;
        }
    }
}
