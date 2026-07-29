using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace ProgramDesigner.DAL.Repositories.GemnericRepository
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(int id);

        Task<IReadOnlyList<T>> GetAllAsync();

        Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate);

        Task AddAsync(T entity);

        void Update(T entity);

        void Remove(T entity);
    }
}
