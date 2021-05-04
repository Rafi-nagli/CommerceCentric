using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Common.Services;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.DbInventory;
using Amazon.Core.Contracts.Factories;
using Amazon.DAL.Inventory;
using log4net;

namespace Amazon.DAL
{
    public class DbFactory : IDbFactory
    {
        public IUnitOfWork GetRWDb()
        {
            return new UnitOfWork(LogFactory.DB);
        }

        public IUnitOfWork GetRDb()
        {
            var uow = new UnitOfWork(LogFactory.DB);
            uow.DisableValidation();
            uow.DisableProxyCreation();
            uow.DisableAutoDetectChanges();
            // Globally
            ////https://github.com/zzzprojects/EntityFramework-Plus/wiki/EF-Audit-%7C-Entity-Framework-Audit-Trail-Context-and-Track-Changes
            //AuditManager.DefaultConfiguration.Exclude(x => true); // Exclude ALL
            //AuditManager.DefaultConfiguration.Include<IAuditable>();

            return uow;
        }

        public IInventoryUnitOfWork GetInventoryRWDb()
        {
            return new InventoryUnitOfWork();
        }

        public IInventoryUnitOfWork GetInventoryRDb()
        {
            var uow = new InventoryUnitOfWork();
            uow.DisableValidation();
            uow.DisableProxyCreation();
            uow.DisableAutoDetectChanges();
            return uow;
        }
    }
}
