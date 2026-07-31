using ProgramDesigner.Domain.Common;
using ProgramDesigner.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramDesigner.Domain
{
    public sealed class Step : ProgramItem
    {
        public string StepType { get; private set; } = string.Empty;
        private Step() { } // for EF Core materialization

        private Step(string name, string stepType) : base(name)
        {
            if (string.IsNullOrWhiteSpace(stepType))
                throw new DomainException("A Step must have a StepType.");

            StepType = stepType;
        }

        public static Step Create(string name, string stepType) => new Step(name, stepType);
    }
}
