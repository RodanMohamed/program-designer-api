using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramDesigner.BLL.DTOs.Responses
{
    // Response body for POST /programs/:id/validate
    public class ValidateProgramResponseDto
    {
        // False only when ImpossiblePrerequisites is non-empty.
        // Reachability warnings never affect this flag.
        public bool IsValid { get; set; }

        public List<PrerequisiteIssueDto> ImpossiblePrerequisites { get; set; } = new List<PrerequisiteIssueDto>();

        public List<PrerequisiteIssueDto> ReachabilityWarnings { get; set; } = new List<PrerequisiteIssueDto>();
    }
}
