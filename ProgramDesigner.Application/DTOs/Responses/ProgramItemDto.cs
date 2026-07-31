using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramDesigner.Application.DTOs.Responses
{
    public class ProgramItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ItemType { get; set; } = string.Empty;
        public string? StepType { get; set; }
        public string? RuleType { get; set; }
        public int? ChoiceCount { get; set; }
        public List<PrerequisiteRefDto> Prerequisites { get; set; } = new();
        public List<ProgramItemDto> Children { get; set; } = new();
    }
}
