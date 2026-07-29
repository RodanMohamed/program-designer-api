using ProgramDesigner.DAL.Repositories.LearningProgramRepository;
using ProgramDesigner.DAL.Repositories.ProgramItemRepository;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramDesigner.DAL.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        ILearningProgramRepository LearningPrograms { get; }

        IProgramItemRepository ProgramItems { get; }

        Task<int> SaveChangesAsync();
    }
}
