using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramDesigner.Application.DTOs.Requests
{
    public class CreateProgramItemDto
    {
        public string Key { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        // Expected values: "Step" or "Group"
        public string ItemType { get; set; } = string.Empty;

        public string? StepType { get; set; }

        // Only used when ItemType == "Group". Expected values: "InOrder" or "Choice"
        public string? RuleType { get; set; }
        public int? ChoiceCount { get; set; }

        // Supports more than one prerequisite (AND semantics): this item unlocks
        // only once every key listed here is completed.
        public List<string> PrerequisiteKeys { get; set; } = new();

        public List<CreateProgramItemDto> Children { get; set; } = new();
    }
}
