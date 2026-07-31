//using Microsoft.EntityFrameworkCore;
//using ProgramDesigner.DAL.Data.Context;
//using ProgramDesigner.DAL.Data.Models;
//using ProgramDesigner.DAL.Repositories.GemnericRepository;
//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace ProgramDesigner.DAL.Repositories.LearningProgramRepository
//{
//    public class LearningProgramRepository : GenericRepository<LearningProgram>, ILearningProgramRepository
//    {
//        public LearningProgramRepository(ApplicationDbContext context) : base(context)
//        {
//        }

//        public async Task<LearningProgram?> GetByIdWithRootGroupAsync(int id)
//        {
//            return await DbSet
//                .Include(p => p.RootGroup)
//                .FirstOrDefaultAsync(p => p.Id == id);
//        }
//    }
//}
