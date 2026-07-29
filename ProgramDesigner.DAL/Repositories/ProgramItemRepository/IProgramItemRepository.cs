using ProgramDesigner.DAL.Data.Models;
using ProgramDesigner.DAL.Repositories.GemnericRepository;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramDesigner.DAL.Repositories.ProgramItemRepository
{
    public interface IProgramItemRepository : IGenericRepository<ProgramItem>
    {
        // Loads every ProgramItem row in one query, with no tracking (read-only use case).
        // The BLL layer builds the in-memory tree from this flat list starting at a given root id.
        // See design note: TPH table has no direct "ProgramId" column, so full-tree loading
        // happens in memory rather than via a single filtered SQL query.
        Task<IReadOnlyList<ProgramItem>> GetAllFlatNoTrackingAsync();
    }
}
