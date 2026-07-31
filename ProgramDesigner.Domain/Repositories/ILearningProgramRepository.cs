using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramDesigner.Domain.Repositories
{
    public interface ILearningProgramRepository
    {
        // Loads the aggregate with its ENTIRE tree already wired up in memory —
        // no separate flat-list + rebuild step is needed anymore.
        Task<LearningProgram?> GetByIdAsync(int id);
        Task<bool> ExistsByNameAsync(string name);

        Task AddAsync(LearningProgram program);

        Task SaveChangesAsync();
    }
}
