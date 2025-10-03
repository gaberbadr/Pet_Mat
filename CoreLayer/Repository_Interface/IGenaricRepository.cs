using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Entities;
using CoreLayer.Specifications;
using Microsoft.EntityFrameworkCore;

namespace CoreLayer.Repository_Interface
{
    public interface IGenaricRepository<TEntity, TKey> where TEntity : BaseEntity<TKey>
    {
        Task<IEnumerable<TEntity>> GetAllAsync();

        Task<IEnumerable<TEntity>> GetAllWithSpecficationAsync(ISpecifications<TEntity, TKey> spec);
        Task<TEntity> GetAsync(TKey id);
        Task<TEntity> GetWithSpecficationAsync(ISpecifications<TEntity, TKey> spec);
        Task<int> GetCountAsync(ISpecifications<TEntity, TKey> spec);

        Task<IEnumerable<TEntity>> GetAllAsNoTrackingAsync();

        Task<TEntity> GetAsNoTrackingAsync(TKey id);

        Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);
        Task<int> DeleteRangeAsync(Expression<Func<TEntity, bool>> predicate);

        Task AddRangeAsync(IEnumerable<TEntity> entities);

        Task AddAsync(TEntity entity);
        void Update(TEntity entity);
        void Delete(TEntity entity);


    }
}
