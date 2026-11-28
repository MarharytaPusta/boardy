using System;
using System.Collections.Generic;

namespace Boardy.Domain.Entities
{
    public class ScoreTemplate
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }

        private readonly List<ScoreCriterion> _criteria = new();
        public IReadOnlyCollection<ScoreCriterion> Criteria => _criteria.AsReadOnly();

        public ScoreTemplate(string name)
        {
            Id = Guid.NewGuid();
            Name = name.Trim();
        }

        private ScoreTemplate() { Name = string.Empty; }

        public void AddCriterion(ScoreCriterion criterion)
        {
            criterion.SetOrder(_criteria.Count);
            _criteria.Add(criterion);
        }

        public void UpdateName(string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
                throw new ArgumentException("Назва шаблону не може бути порожньою.", nameof(newName));

            Name = newName;
        }

        public void ClearCriteria()
        {
            _criteria.Clear();
        }
    }
}