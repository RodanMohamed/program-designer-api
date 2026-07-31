using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramDesigner.Domain.Requests
{
    // Input for the simulation Domain Service: what a specific participant has
    // completed and chosen so far. Ids reference already-persisted ProgramItems,
    // since simulation only ever runs against a saved LearningProgram.
    public sealed class SimulationInput
    {
        // Key: a Choice Group's Id. Value: the Ids of the children the participant picked inside it.
        public IReadOnlyDictionary<int, IReadOnlyList<int>> Choices { get; }

        // Ids of Steps the participant has already completed.
        public IReadOnlySet<int> CompletedItemIds { get; }

        public SimulationInput(IReadOnlyDictionary<int, IReadOnlyList<int>> choices, IReadOnlySet<int> completedItemIds)
        {
            Choices = choices;
            CompletedItemIds = completedItemIds;
        }
    }
}
