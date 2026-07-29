using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramDesigner.BLL.DTOs.Responses
{
    // Describes a single problem found during validation, whether it's a
    // blocking error (impossible prerequisite) or a non-blocking warning (unreachable risk).
    public class PrerequisiteIssueDto
    {
        public int ItemId { get; set; }

        public string ItemName { get; set; } = string.Empty;

        public int PrerequisiteItemId { get; set; }

        public string PrerequisiteItemName { get; set; } = string.Empty;

        // Human-readable explanation
        public string Description { get; set; } = string.Empty;
    }
}
