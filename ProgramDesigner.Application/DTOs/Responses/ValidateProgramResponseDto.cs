using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramDesigner.Application.DTOs.Responses
{
    public class ValidateProgramResponseDto
    {
        public bool IsValid { get; set; }
        public List<PrerequisiteIssueDto> ImpossiblePrerequisites { get; set; } = new();
        public List<PrerequisiteIssueDto> ReachabilityWarnings { get; set; } = new();
    }
}
