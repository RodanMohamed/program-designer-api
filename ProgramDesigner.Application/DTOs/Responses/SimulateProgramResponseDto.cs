using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramDesigner.Application.DTOs.Responses
{
    public class SimulateProgramResponseDto
    {
        public List<ProgramItemStateDto> Items { get; set; } = new();
    }
}
