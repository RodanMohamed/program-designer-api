//using Microsoft.EntityFrameworkCore;
//using ProgramDesigner.DAL.Data.Context;
//using System;
//using System.Collections.Generic;
//using System.Linq.Expressions;
//using System.Text;

//namespace ProgramDesigner.DAL.Repositories.GemnericRepository
//{
//    public class GenericRepository<T> : IGenericRepository<T> where T : class
//    {
//        protected readonly ApplicationDbContext Context;
//        protected readonly DbSet<T> DbSet;

//        public GenericRepository(ApplicationDbContext context)
//        {
//            Context = context;
//            DbSet = context.Set<T>();
//        }

//        public async Task<T?> GetByIdAsync(int id)
//        {
//            return await DbSet.FindAsync(id);
//        }

//        public async Task<IReadOnlyList<T>> GetAllAsync()
//        {
//            return await DbSet.ToListAsync();
//        }

//        public async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate)
//        {
//            return await DbSet.Where(predicate).ToListAsync();
//        }

//        public async Task AddAsync(T entity)
//        {
//            await DbSet.AddAsync(entity);
//        }

//        public void Update(T entity)
//        {
//            DbSet.Update(entity);
//        }

//        public void Remove(T entity)
//        {
//            DbSet.Remove(entity);
//        }
//    }
//}
