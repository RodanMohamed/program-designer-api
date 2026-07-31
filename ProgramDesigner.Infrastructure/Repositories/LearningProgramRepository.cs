using Microsoft.EntityFrameworkCore;
using ProgramDesigner.Domain;
using ProgramDesigner.Domain.Repositories;
using ProgramDesigner.Infrastructure.Data.Context;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProgramDesigner.Infrastructure.Repositories
{
    public class LearningProgramRepository : ILearningProgramRepository
    {
        private readonly ApplicationDbContext _context;

        public LearningProgramRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<LearningProgram?> GetByIdAsync(int id)
        {
            LearningProgram? program = await _context.LearningPrograms
                .Include(p => p.RootGroup)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (program == null)
            {
                return null;
            }

            // Breadth-first load: fetch each level's children in one query, then use
            // whichever of those turned out to be Groups as the parents for the next
            // level. The context keeps tracking everything, so EF's relationship
            // fixup wires Children/ParentGroup automatically as each batch arrives —
            // no manual tree-building code, and it works no matter how deep the
            // program nests.
            List<int> currentLevelParentIds = new() { program.RootGroup.Id };

            while (currentLevelParentIds.Count > 0)
            {
                List<ProgramItem> nextLevel = await _context.ProgramItems
                    .Where(i => EF.Property<int?>(i, "ParentGroupId") != null
                        && currentLevelParentIds.Contains(EF.Property<int?>(i, "ParentGroupId")!.Value))
                    .Include(i => i.Prerequisites)
                    .ToListAsync();

                currentLevelParentIds = nextLevel.OfType<Group>().Select(g => g.Id).ToList();
            }

            return program;
        }

        public async Task AddAsync(LearningProgram program)
        {
            // Adding the aggregate root is enough — EF Core walks the whole reachable
            // object graph (RootGroup, its Children, their Children, ...) through the
            // configured navigations and marks every new entity as Added automatically.
            await _context.LearningPrograms.AddAsync(program);
        }
        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await _context.LearningPrograms.AnyAsync(p => p.Name == name);
        }

        public Task SaveChangesAsync() => _context.SaveChangesAsync();

    }
}
