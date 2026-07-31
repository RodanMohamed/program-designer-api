using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramDesigner.Application.DTOs.Requests
{
    public class CreateProgramDto
    {
        public string Name { get; set; } = string.Empty;
        public CreateProgramItemDto RootGroup { get; set; } = new();
    }
}
