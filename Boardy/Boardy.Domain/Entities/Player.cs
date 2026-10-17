using System;

namespace Boardy.Domain.Entities
{
    public class Player
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }
}
