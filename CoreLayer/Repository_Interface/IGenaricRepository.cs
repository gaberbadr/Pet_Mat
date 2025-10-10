using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CoreLayer.Entities;
using CoreLayer.Specifications;
using Microsoft.EntityFrameworkCore;

namespace CoreLayer.Repository_Interface
{
    public interface IGenaricRepository<TEntity, TKey> where TEntity : BaseEntity<TKey>
    {
        // ==================== BASIC CRUD OPERATIONS ====================

        Task<IEnumerable<TEntity>> GetAllAsync();
        Task<TEntity> GetAsync(TKey id);
        Task<IEnumerable<TEntity>> GetAllAsNoTrackingAsync();
        Task<TEntity> GetAsNoTrackingAsync(TKey id);
        Task AddAsync(TEntity entity);
        void Update(TEntity entity);
        void Delete(TEntity entity);

        // ==================== QUERY OPERATIONS ====================
        Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);
        Task<int> DeleteRangeAsync(Expression<Func<TEntity, bool>> predicate);
        Task AddRangeAsync(IEnumerable<TEntity> entities);

        // ==================== SPECIFICATION PATTERN OPERATIONS ====================


        Task<int> GetCountAsync(ISpecifications<TEntity, TKey> spec);
        Task<IEnumerable<TEntity>> GetAllWithSpecficationAsync(ISpecifications<TEntity, TKey> spec);
        Task<TEntity> GetWithSpecficationAsync(ISpecifications<TEntity, TKey> spec);
        Task<IEnumerable<TDto>> GetAllWithProjectionAsync<TDto>(
            ISpecifications<TEntity, TKey> spec,
            IConfigurationProvider mapperConfig);

    
}
}
