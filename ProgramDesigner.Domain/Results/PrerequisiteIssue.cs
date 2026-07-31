using ProgramDesigner.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramDesigner.Domain.Results
{
    // Describes one problem found on a single (Item, Prerequisite) pair.
    // Holds direct references to the Domain entities involved; the Application
    // layer is responsible for mapping these to ids/DTOs for the API response.
    public sealed class PrerequisiteIssue
    {
        public ProgramItem Item { get; }
        public ProgramItem Prerequisite { get; }
        public string Description { get; }

        public PrerequisiteIssue(ProgramItem item, ProgramItem prerequisite, string description)
        {
            Item = item;
            Prerequisite = prerequisite;
            Description = description;
        }
    }
}
