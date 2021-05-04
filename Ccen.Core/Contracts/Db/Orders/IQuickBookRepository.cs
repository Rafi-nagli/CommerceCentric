using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Inventory;
using Amazon.Core.Entities.Orders;
using Amazon.Core.Entities.Quickbooks;
using Amazon.Core.Models;
using Amazon.Core.Models.Settings;
using Amazon.DTO;
using Amazon.DTO.Orders;
using Amazon.DTO.Shippings;

namespace Amazon.Core.Contracts.Db
{
    public interface IQuickBookRepository : IRepository<QuickbooksInventory>
    {
        IQueryable<QuickBookExportDTO> GetAllExportAsDto();

        IQueryable<ItemDTO> GetInventoryAsDto();

        IQueryable<ProductList> GetAllProductList();
    }
}
