using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramDesigner.BLL.DTOs.Responses
{
    public class ProgramItemStateDto
    {
        public int ItemId { get; set; }

        public string ItemName { get; set; } = string.Empty;

        public string ItemType { get; set; } = string.Empty;

        // "Complete", "Unlocked", "Blocked", or "Excluded"
        public string Status { get; set; } = string.Empty;

        // Human-readable explanation, populated only for Blocked and Excluded statuses.
        public string? Reason { get; set; }
    }
}
