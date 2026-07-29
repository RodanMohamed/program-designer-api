using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramDesigner.BLL.DTOs.Requests
{
    // Top-level request body for POST /programs.
    // RootGroup must always have ItemType == "Group" (a program is one top-level group).
    public class CreateProgramDto
    {
        public string Name { get; set; } = string.Empty;

        public CreateProgramItemDto RootGroup { get; set; } = new CreateProgramItemDto();
    }
}
