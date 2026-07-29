using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramDesigner.BLL.DTOs.Responses
{
    // Top-level response for both POST /programs and GET /programs/:id.
    public class ProgramDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public ProgramItemDto RootGroup { get; set; } = new ProgramItemDto();
    }
}
