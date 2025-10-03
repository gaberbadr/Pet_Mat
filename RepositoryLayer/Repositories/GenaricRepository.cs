using System;
using System.Linq.Expressions;
using CoreLayer.Entities;
using CoreLayer.Repository_Interface;
using CoreLayer.Specifications;
using Microsoft.EntityFrameworkCore;
using RepositoryLayer;
using RepositoryLayer.Data.Context;

public class GenaricRepository<TEntity, TKey> : IGenaricRepository<TEntity, TKey> where TEntity : BaseEntity<TKey>
{
    private readonly ApplicationDbContext _dbContext;
    public GenaricRepository(ApplicationDbContext dBContext)
    {
        _dbContext = dBContext;
    }

    public async Task<IEnumerable<TEntity>> GetAllAsync()
    {

        return await _dbContext.Set<TEntity>().ToListAsync();
    }

    public async Task<TEntity> GetAsync(TKey id)
    {

        return await _dbContext.Set<TEntity>().FindAsync(id);
    }

    public async Task<IEnumerable<TEntity>> GetAllAsNoTrackingAsync()
    {
        return await _dbContext.Set<TEntity>().AsNoTracking().ToListAsync();
    }

    public async Task<TEntity> GetAsNoTrackingAsync(TKey id)
    {
        var entity = await _dbContext.Set<TEntity>().FindAsync(id);
        if (entity != null)
        {
            _dbContext.Entry(entity).State = EntityState.Detached;
        }
        return entity;
        /*Entity States:
        Detached - Entity is not tracked by the context
        Unchanged - Entity is tracked but hasn't been modified
        Added - Entity is new and will be inserted
        Modified - Entity is tracked and has been changed
        Deleted - Entity is tracked and will be deleted*/
    }
    public async Task AddAsync(TEntity entity)
    {
        await _dbContext.Set<TEntity>().AddAsync(entity);
    }

    public void Update(TEntity entity)
    {
        _dbContext.Set<TEntity>().Update(entity);
    }
    public void Delete(TEntity entity)
    {
        _dbContext.Set<TEntity>().Remove(entity);
    }



    //Find entities by expression (e.g., Id = 4, Name = "John", Age > 18,x => x.Age > 25 && x.IsActive)
    public async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await _dbContext.Set<TEntity>().Where(predicate).ToListAsync();
    }

    //Delete all entities matching the expression (e.g., Id = 6, Name = "Test")
    public async Task<int> DeleteRangeAsync(Expression<Func<TEntity, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        var entitiesToDelete = await _dbContext.Set<TEntity>().Where(predicate).ToListAsync();

        if (entitiesToDelete.Any())
        {
            _dbContext.Set<TEntity>().RemoveRange(entitiesToDelete);
        }

        return entitiesToDelete.Count;
    }

    public async Task AddRangeAsync(IEnumerable<TEntity> entities)
    {
        await _dbContext.Set<TEntity>().AddRangeAsync(entities);
    }

    //Refactory function
    private IQueryable<TEntity> ApplySpecfications(ISpecifications<TEntity, TKey> spec)
    {
        return SpecificationsEvaluator<TEntity, TKey>.GetQuery(_dbContext.Set<TEntity>(), spec);
    }

    public async Task<int> GetCountAsync(ISpecifications<TEntity, TKey> spec)
    {
        return await ApplySpecfications(spec).CountAsync();
        // return await SpecificationsEvaluator<TEntity, TKey>.GetQuery(_dbContext.Set<TEntity>(), spec).CountAsync();
    }
    public async Task<IEnumerable<TEntity>> GetAllWithSpecficationAsync(ISpecifications<TEntity, TKey> spec)
    {
        return await ApplySpecfications(spec).ToListAsync();
    }

    public async Task<TEntity> GetWithSpecficationAsync(ISpecifications<TEntity, TKey> spec)
    {
        return await ApplySpecfications(spec).FirstOrDefaultAsync();
    }

}