//using ProgramDesigner.DAL.Data.Context;
//using ProgramDesigner.DAL.Repositories.LearningProgramRepository;
//using ProgramDesigner.DAL.Repositories.ProgramItemRepository;
//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace ProgramDesigner.DAL.UnitOfWork
//{
//    public class UnitOfWork : IUnitOfWork
//    {
//        private readonly ApplicationDbContext _context;

//        private ILearningProgramRepository? _learningPrograms;
//        private IProgramItemRepository? _programItems;

//        public UnitOfWork(ApplicationDbContext context)
//        {
//            _context = context;
//        }

//        public ILearningProgramRepository LearningPrograms
//        {
//            get
//            {
//                if (_learningPrograms == null)
//                {
//                    _learningPrograms = new LearningProgramRepository(_context);
//                }

//                return _learningPrograms;
//            }
//        }

//        public IProgramItemRepository ProgramItems
//        {
//            get
//            {
//                if (_programItems == null)
//                {
//                    _programItems = new ProgramItemRepository(_context);
//                }

//                return _programItems;
//            }
//        }

//        public async Task<int> SaveChangesAsync()
//        {
//            return await _context.SaveChangesAsync();
//        }

//        public void Dispose()
//        {
//            _context.Dispose();
//            GC.SuppressFinalize(this);
//        }
//    }
//}
