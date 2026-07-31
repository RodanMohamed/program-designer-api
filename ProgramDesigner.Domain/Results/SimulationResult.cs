using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramDesigner.Domain.Results
{
    public sealed class SimulationResult
    {
        // Every item in the program, ordered top-to-bottom the way a designer would read the tree.
        public IReadOnlyList<ProgramItemState> Items { get; }

        public SimulationResult(IReadOnlyList<ProgramItemState> items)
        {
            Items = items;
        }
    }
}
