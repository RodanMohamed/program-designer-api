using ProgramDesigner.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace ProgramDesigner.Domain
{
    // Abstract base for Step and Group. Not an Aggregate Root on its own —
    // it only ever exists as part of the LearningProgram aggregate.
    public abstract class ProgramItem : Entity
    {
        protected ProgramItem() { }
        protected ProgramItem(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("An item's Name cannot be empty.");

            Name = name;
        }
        public string Name { get; protected set; } = string.Empty;

        // Position among siblings inside the same parent Group.
        public int Order { get; internal set; }

        public Group? ParentGroup { get; internal set; }

        private readonly List<ProgramItem> _prerequisites = new();
        // All prerequisites must be completed (AND semantics) before this item unlocks.
        public IReadOnlyCollection<ProgramItem> Prerequisites => _prerequisites.AsReadOnly();

        public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;

      

        // Only the immediate, local invariant is enforced here (self-reference).
        // Structural checks that need the whole tree (nested/appears-later/reachability)
        // are the responsibility of the validation Domain Service.
        public void AddPrerequisite(ProgramItem prerequisite)
        {
            if (prerequisite == null)
                throw new ArgumentNullException(nameof(prerequisite));

            if (prerequisite == this)
                throw new DomainException($"'{Name}' cannot have a prerequisite pointing at itself.");

            if (_prerequisites.Contains(prerequisite))
                return; // idempotent: adding the same prerequisite twice is a no-op, not an error

            _prerequisites.Add(prerequisite);
        }
    }
}
