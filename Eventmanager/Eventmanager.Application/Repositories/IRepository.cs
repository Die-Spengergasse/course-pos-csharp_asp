using Eventmanager.Application.Model;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Eventmanager.Application.Repositories
{
    public interface IRepository<TEntity> where TEntity : Entity
    {
        Task<TEntity?> FindByIdAsync(int id);
        Task<TOtherEntity?> FindByIdAsync<TOtherEntity>(int id, params string[] includeNavigations) where TOtherEntity : Entity;
        Task<TOtherEntity?> FindByIdAsync<TOtherEntity>(int id) where TOtherEntity : Entity;
        Task<TEntity?> FindByIdAsync(int id, params string[] includeNavigations);
        Task<TEntity> CreateAndSave(TEntity entity);
        Task DeleteAndSave(TEntity entity);
        Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate);
        Tresult Query<Tresult>(Func<IQueryable<TEntity>, Tresult> queryFunc);
        Task UpdateAndSave(TEntity entity);
    }
}