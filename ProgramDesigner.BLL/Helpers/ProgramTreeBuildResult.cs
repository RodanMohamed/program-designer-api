using ProgramDesigner.DAL.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramDesigner.BLL.Helpers
{
    // Bundles everything produced by building the in-memory tree from a flat list,
    // so the validation engine can use the lookup dictionaries directly without recomputation.
    public class ProgramTreeBuildResult
    {
        public ProgramItem Root { get; set; } = null!;

        // Fast lookup: any item by its real database Id.
        public Dictionary<int, ProgramItem> ItemsById { get; set; } = new Dictionary<int, ProgramItem>();

        // DFS pre-order numbering: the moment we first visit this item.
        public Dictionary<int, int> EnterOrder { get; set; } = new Dictionary<int, int>();

        // DFS pre-order numbering: the moment we finish visiting this item and all its descendants.
        public Dictionary<int, int> ExitOrder { get; set; } = new Dictionary<int, int>();
    }
}
