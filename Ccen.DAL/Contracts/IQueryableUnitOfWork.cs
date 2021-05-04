using System.Data.Entity;
using Amazon.Core;
using Amazon.DAL;

namespace Amazon.Core
{
    public interface IQueryableUnitOfWork : IUnitOfWork, ISql
    {
        AmazonContext Context { get; }

        DbSet<TEntity> GetSet<TEntity>() where TEntity : class;
        void Attach<TEntity>(TEntity item) where TEntity : class;
        void SetModified<TEntity>(TEntity item) where TEntity : class;
        void ApplyCurrentValues<TEntity>(TEntity original, TEntity current) where TEntity : class;
    }
}
