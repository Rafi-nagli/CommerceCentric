using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using Amazon.Core;
using Amazon.Core.Contracts.DbInventory;

namespace Amazon.DAL.Inventory.Contracts
{
    public interface IQueryableUnitOfWork : IInventoryUnitOfWork, ISql
    {
        InventoryContext Context { get; }

        DbSet<TEntity> GetSet<TEntity>() where TEntity : class;
        void Attach<TEntity>(TEntity item) where TEntity : class;
        void SetModified<TEntity>(TEntity item) where TEntity : class;
        void ApplyCurrentValues<TEntity>(TEntity original, TEntity current) where TEntity : class;
    }
}
