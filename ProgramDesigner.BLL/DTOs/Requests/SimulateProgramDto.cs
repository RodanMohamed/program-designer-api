using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramDesigner.BLL.DTOs.Requests
{
   
    public class SimulateProgramDto
    {
       
        public Dictionary<int, List<int>> Choices { get; set; } = new Dictionary<int, List<int>>();

        // Ids of Steps the participant has already completed.
        public List<int> CompletedItemIds { get; set; } = new List<int>();
    }
}
