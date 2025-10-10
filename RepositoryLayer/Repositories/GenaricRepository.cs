using System;
using System.Linq.Expressions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using CoreLayer.Entities;
using CoreLayer.Repository_Interface;
using CoreLayer.Specifications;
using Microsoft.EntityFrameworkCore;
using RepositoryLayer;
using RepositoryLayer.Data.Context;

public class GenaricRepository<TEntity, TKey> : IGenaricRepository<TEntity, TKey>
      where TEntity : BaseEntity<TKey>
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

    public async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await _dbContext.Set<TEntity>().Where(predicate).ToListAsync();
    }

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

    private IQueryable<TEntity> ApplySpecfications(ISpecifications<TEntity, TKey> spec)
    {
        return SpecificationsEvaluator<TEntity, TKey>.GetQuery(_dbContext.Set<TEntity>(), spec);
    }

    public async Task<int> GetCountAsync(ISpecifications<TEntity, TKey> spec)
    {
        return await ApplySpecfications(spec).CountAsync();
    }

    public async Task<IEnumerable<TEntity>> GetAllWithSpecficationAsync(ISpecifications<TEntity, TKey> spec)
    {
        return await ApplySpecfications(spec).ToListAsync();
    }

    public async Task<TEntity> GetWithSpecficationAsync(ISpecifications<TEntity, TKey> spec)
    {
        return await ApplySpecfications(spec).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<TDto>> GetAllWithProjectionAsync<TDto>(ISpecifications<TEntity, TKey> spec, IConfigurationProvider mapperConfig)
    {
        return await ApplySpecfications(spec)
            .AsNoTracking()
            .ProjectTo<TDto>(mapperConfig)
            .ToListAsync();
    }

}