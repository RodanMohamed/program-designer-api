using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramDesigner.Common.Enums
{
    // Represents a participant's current relationship to a single ProgramItem
    // at a given point in the simulation (not persisted — computed on the fly).
    public enum ProgramItemStatus
    {
        // Already done 
        Complete = 1,

        // Can be attempted right now: prerequisite satisfied, in-order sequence respected,
        // and not sitting inside an unselected choice branch.
        Unlocked = 2,

        // Not yet available: waiting on a prerequisite, an earlier in-order sibling,
        // or a parent group that is itself Blocked/Excluded.
        Blocked = 3,

        // Sits inside a choice branch the participant did not select — can never be completed.
        Excluded = 4
    }
}
