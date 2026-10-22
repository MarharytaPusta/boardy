using System;

namespace Boardy.Domain.Entities
{
    public class ScoreCriterion
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public bool IsNegative { get; private set; }

        public ScoreCriterion(string name, bool isNegative = false)
        {
            Id = Guid.NewGuid();
            Name = name.Trim();
            IsNegative = isNegative;
        }

        private ScoreCriterion() { Name = string.Empty; }
    }
}