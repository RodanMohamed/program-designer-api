using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramDesigner.Common.Enums
{
    public enum GroupRuleType
    {
        // All children must be completed, no choice
        InOrder = 1,

        // Participant must complete N out of M children (M = children count)
        Choice = 2
    }
}
