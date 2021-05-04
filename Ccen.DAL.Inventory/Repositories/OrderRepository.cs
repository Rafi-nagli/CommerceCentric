using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core.Contracts.DbInventory;
using Amazon.Core.EntitiesInventory;
using Amazon.DAL.Inventory.Contracts;

namespace Amazon.DAL.Inventory.Repositories
{
    public class OrderRepository : Repository<Order>, IOrderRepository
    {
        public OrderRepository(IQueryableUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
    }
}
