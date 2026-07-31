using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramDesigner.Application.DTOs.Requests
{
    public class SimulateProgramDto
    {
        public Dictionary<int, List<int>> Choices { get; set; } = new();
        public List<int> CompletedItemIds { get; set; } = new();
    }

}
