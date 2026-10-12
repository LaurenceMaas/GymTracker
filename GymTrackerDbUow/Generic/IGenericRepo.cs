using System.Linq.Expressions;

namespace GymTrackerDbUow.Generic
{
    public interface IGenericRepo<T>
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> GetByIdAsync(object id);
        Task<int> InsertAsync(T obj);
        Task UpdateAsync(T obj);
        Task DeleteAsync(object id);
        Task<IReadOnlyList<T>> QueryAsync(Expression<Func<T, bool>> predicate, bool trackChanges, params string[] includePaths);
        void StageAdd(T entity);
        void StageRemove(T entity);
        Task<int> SaveAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
