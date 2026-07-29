using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramDesigner.BLL.DTOs.Responses
{
    public class SimulateProgramResponseDto
    {
        // Every item in the program, ordered top-to-bottom the way a designer would read the tree.
        public List<ProgramItemStateDto> Items { get; set; } = new List<ProgramItemStateDto>();
    }
}
