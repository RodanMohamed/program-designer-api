using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramDesigner.DAL.Data.Models
{
    public sealed class Step : ProgramItem
    {
        public string StepType { get; set; } = string.Empty;
    }
}
