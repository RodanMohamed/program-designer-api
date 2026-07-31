using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramDesigner.Domain.Results
{
    public sealed class ValidationResult
    {
        public bool IsValid { get; }
        public IReadOnlyList<PrerequisiteIssue> ImpossiblePrerequisites { get; }
        public IReadOnlyList<PrerequisiteIssue> ReachabilityWarnings { get; }

        public ValidationResult(
            bool isValid,
            IReadOnlyList<PrerequisiteIssue> impossiblePrerequisites,
            IReadOnlyList<PrerequisiteIssue> reachabilityWarnings)
        {
            IsValid = isValid;
            ImpossiblePrerequisites = impossiblePrerequisites;
            ReachabilityWarnings = reachabilityWarnings;
        }
    }
}
