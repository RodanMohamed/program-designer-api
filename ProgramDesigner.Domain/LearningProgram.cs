using ProgramDesigner.Domain.Common;
using ProgramDesigner.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramDesigner.Domain
{
    // The Aggregate Root. Everything inside a program (Groups, Steps, their
    // prerequisites) is only ever reached and modified through this root.
    public sealed class LearningProgram : Entity
    {
        public string Name { get; private set; } = string.Empty;
        public Group RootGroup { get; private set; } = null!;
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

        private LearningProgram() { } // for EF Core materialization

        private LearningProgram(string name, Group rootGroup)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("A program's Name cannot be empty.");

            if (rootGroup == null)
                throw new ArgumentNullException(nameof(rootGroup));

            rootGroup.EnsureRuleConsistencyRecursively();

            Name = name;
            RootGroup = rootGroup;
        }

        public static LearningProgram Create(string name, Group rootGroup) =>
            new LearningProgram(name, rootGroup);

    }
}
