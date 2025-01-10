// =================================================================================================
// Generic repository for ef core with pragmatic find functions.
// Michael Schletz, 2026-03-13
// =================================================================================================
using Eventmanager.Application.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Eventmanager.Application.Repositories;

public class Repository<TEntity> : IRepository<TEntity> where TEntity : Entity
{
    protected readonly DbContext db;

    public Repository(DbContext db)
    {
        this.db = db;
    }

    /// <summary>
    /// Finds an entity of a specified type by its identifier. 
    /// Useful for quickly resolving foreign key constraints without needing a separate repository.
    /// </summary>
    public virtual Task<TOtherEntity?> FindByIdAsync<TOtherEntity>(int id)
        where TOtherEntity : Entity
        => db.Set<TOtherEntity>().FirstOrDefaultAsync(e => e.Id == id);

    /// <summary>
    /// Finds an entity of the repository's primary type by its identifier.
    /// </summary>
    public virtual Task<TEntity?> FindByIdAsync(int id)
        => db.Set<TEntity>().FirstOrDefaultAsync(e => e.Id == id);

    /// <summary>
    /// Finds an entity of a specified type by its identifier and eagerly loads the specified navigation properties.
    /// </summary>
    public virtual Task<TOtherEntity?> FindByIdAsync<TOtherEntity>(
        int id,
        params string[] includeNavigations)
        where TOtherEntity : Entity
    {
        var query = db.Set<TOtherEntity>().AsQueryable();
        foreach (var include in includeNavigations)
            query = query.Include(include);

        return query.FirstOrDefaultAsync(e => e.Id == id);
    }

    /// <summary>
    /// Finds an entity of the repository's primary type by its identifier and eagerly loads the specified navigation properties.
    /// </summary>
    public virtual Task<TEntity?> FindByIdAsync(int id, params string[] includeNavigations)
        => FindByIdAsync<TEntity>(id, includeNavigations);

    /// <summary>
    /// Asynchronously determines whether any entity of the repository's primary type satisfies the specified condition.
    /// </summary>
    public virtual Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate)
        => db.Set<TEntity>().AnyAsync(predicate);

    /// <summary>
    /// Executes a custom query function against the primary entity set.
    /// </summary>
    public virtual Tresult Query<Tresult>(Func<IQueryable<TEntity>, Tresult> queryFunc)
        => queryFunc(db.Set<TEntity>());

    /// <summary>
    /// Adds a new entity to the database context and saves the changes.
    /// </summary>
    public virtual async Task<TEntity> CreateAndSave(TEntity entity)
    {
        db.Entry(entity).State = EntityState.Added;
        await db.SaveChangesAsync();
        return entity;
    }

    /// <summary>
    /// Marks an existing entity as modified and saves the changes to the database.
    /// </summary>
    public virtual async Task UpdateAndSave(TEntity entity)
    {
        db.Entry(entity).State = EntityState.Modified;
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Marks an existing entity as deleted and saves the changes to the database.
    /// </summary>
    public virtual async Task DeleteAndSave(TEntity entity)
    {
        db.Entry(entity).State = EntityState.Deleted;
        await db.SaveChangesAsync();
    }
}
