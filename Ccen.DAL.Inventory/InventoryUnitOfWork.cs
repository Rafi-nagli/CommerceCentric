using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using Amazon.Core.Contracts.DbInventory;
using Amazon.DAL.Inventory.Contracts;
using Amazon.DAL.Inventory.Repositories;

namespace Amazon.DAL.Inventory
{
    public class InventoryUnitOfWork : IQueryableUnitOfWork
    {
        private InventoryContext context;

        public InventoryContext Context
        {
            get
            {
                if (context == null)
                {
                    context = new InventoryContext();
                }
                return context;
            }
        }

        public IItemOrderMappingRepository ItemOrderMappings
        {
            get { return new ItemOrderMappingRepository(this); }
        }

        public IItemInventoryMappingRepository ItemInventoryMappings
        {
            get { return new ItemInventoryMappingRepository(this); }
        }

        public IOrderRepository Orders
        {
            get { return new OrderRepository(this); }
        }

        #region IQueryableUnitOfWork
        public void Dispose()
        {
            if (context != null)
            {
                context.Dispose();
            }
        }


        public void DisableProxyCreation()
        {
            Context.Configuration.ProxyCreationEnabled = false;
        }

        public void DisableAutoDetectChanges()
        {
            Context.Configuration.AutoDetectChangesEnabled = false;
        }

        public void EnableValidation()
        {
            //Context.Configuration.AutoDetectChangesEnabled = true;
            Context.Configuration.ValidateOnSaveEnabled = true;
            //Context.Configuration.ProxyCreationEnabled = true;
        }

        public void DisableValidation()
        {
            //Context.Configuration.AutoDetectChangesEnabled = false;
            Context.Configuration.ValidateOnSaveEnabled = false;
            //Context.Configuration.ProxyCreationEnabled = false;
        }

        public void Commit()
        {
            Context.SaveChanges();
        }

        public void CommitAndRefreshChanges()
        {
            throw new NotSupportedException();
        }

        public void RollbackChanges()
        {
            throw new NotSupportedException();
        }

        public IEnumerable<T> ExecuteQuery<T>(string sqlQuery, params object[] parameters)
        {
            return Context.Database.SqlQuery<T>(sqlQuery, parameters).ToList();
        }

        public int ExecuteCommand(string sqlCommand, params object[] parameters)
        {
            return Context.Database.ExecuteSqlCommand(sqlCommand, parameters);
        }

        public DbSet<TEntity> GetSet<TEntity>() where TEntity : class
        {
            return Context.Set<TEntity>();
        }

        public void Attach<TEntity>(TEntity item) where TEntity : class
        {
            GetSet<TEntity>().Attach(item);
        }

        public void SetModified<TEntity>(TEntity item) where TEntity : class
        {
            throw new NotSupportedException();
        }

        public void ApplyCurrentValues<TEntity>(TEntity original, TEntity current) where TEntity : class
        {
            throw new NotSupportedException();
        }
        #endregion

    }
}
