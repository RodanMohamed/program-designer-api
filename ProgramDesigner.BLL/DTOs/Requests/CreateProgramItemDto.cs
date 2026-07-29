using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramDesigner.BLL.DTOs.Requests
{

    // Represents one node (Step or Group) inside the incoming program tree.
    // "Key" is a temporary, request-scoped identifier chosen by the client,
    // used only to resolve PrerequisiteKey references before real DB ids exist.
    public class CreateProgramItemDto
    {
        public string Key { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        // Expected values: "Step" or "Group"
        public string ItemType { get; set; } = string.Empty;

        // Only used when ItemType == "Step"
        public string? StepType { get; set; }

        // Only used when ItemType == "Group". Expected values: "InOrder" or "Choice"
        public string? RuleType { get; set; }

        // Only used when ItemType == "Group" and RuleType == "Choice"
        public int? ChoiceCount { get; set; }

        // References another node's Key anywhere in the same request tree.
        // Null means this item has no prerequisite.
        public string? PrerequisiteKey { get; set; }

        // Only used when ItemType == "Group"
        public List<CreateProgramItemDto> Children { get; set; } = new List<CreateProgramItemDto>();
    }
}
