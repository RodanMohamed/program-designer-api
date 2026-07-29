using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramDesigner.BLL.DTOs.Responses
{
    public class ProgramItemDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string ItemType { get; set; } = string.Empty;

        public string? StepType { get; set; }

        public string? RuleType { get; set; }

        public int? ChoiceCount { get; set; }

        public int? PrerequisiteItemId { get; set; }

        public string? PrerequisiteItemName { get; set; }

        public List<ProgramItemDto> Children { get; set; } = new List<ProgramItemDto>();
    }
}
