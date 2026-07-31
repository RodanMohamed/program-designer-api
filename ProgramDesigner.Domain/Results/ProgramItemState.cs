using ProgramDesigner.Domain.Common;
using ProgramDesigner.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramDesigner.Domain.Results
{
    public sealed class ProgramItemState
    {
        public ProgramItem Item { get; }
        public ProgramItemStatus Status { get; }
        public string? Reason { get; }

        public ProgramItemState(ProgramItem item, ProgramItemStatus status, string? reason)
        {
            Item = item;
            Status = status;
            Reason = reason;
        }
    }
}
