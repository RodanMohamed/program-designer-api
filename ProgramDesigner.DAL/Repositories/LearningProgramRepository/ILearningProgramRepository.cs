using ProgramDesigner.DAL.Data.Models;
using ProgramDesigner.DAL.Repositories.GemnericRepository;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramDesigner.DAL.Repositories.LearningProgramRepository
{
    public interface ILearningProgramRepository : IGenericRepository<LearningProgram>
    {
        // Loads the program row together with its RootGroup reference
        // (just the root row itself, not the full nested tree — that's built separately
        // by combining this with IProgramItemRepository.GetAllFlatNoTrackingAsync).
        Task<LearningProgram?> GetByIdWithRootGroupAsync(int id);
    }
}
