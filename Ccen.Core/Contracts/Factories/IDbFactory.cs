using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core.Contracts.DbInventory;

namespace Amazon.Core.Contracts.Factories
{
    public interface IDbFactory
    {
        IUnitOfWork GetRWDb();
        IUnitOfWork GetRDb();

        IInventoryUnitOfWork GetInventoryRDb();
        IInventoryUnitOfWork GetInventoryRWDb();
    }
}
