using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramDesigner.Application.DTOs.Responses
{
    public class PrerequisiteIssueDto
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public int PrerequisiteItemId { get; set; }
        public string PrerequisiteItemName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
