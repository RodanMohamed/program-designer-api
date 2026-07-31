using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramDesigner.Domain.Enums
{
    
    public enum ProgramItemStatus
    {
        Complete = 1,   // Already done
        Unlocked = 2,   // Can be attempted right now
        Blocked = 3,    // Waiting on a prerequisite, sequence, or a blocked/excluded parent
        Excluded = 4    // Sits inside a choice branch the participant didn't select
    }
}
