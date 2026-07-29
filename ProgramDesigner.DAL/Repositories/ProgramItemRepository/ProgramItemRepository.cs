using Microsoft.EntityFrameworkCore;
using ProgramDesigner.DAL.Data.Context;
using ProgramDesigner.DAL.Data.Models;
using ProgramDesigner.DAL.Repositories.GemnericRepository;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramDesigner.DAL.Repositories.ProgramItemRepository
{
    public class ProgramItemRepository : GenericRepository<ProgramItem>, IProgramItemRepository
    {
        public ProgramItemRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IReadOnlyList<ProgramItem>> GetAllFlatNoTrackingAsync()
        {
            return await DbSet.AsNoTracking().ToListAsync();
        }
    }
}
