using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Entities.Inventory;
using Amazon.DTO.Inventory;

namespace Amazon.Core.Contracts.Db.Inventory
{
    public interface IStyleItemSaleRepository : IRepository<StyleItemSale>
    {
        IQueryable<StyleItemSaleDTO> GetAllAsDto();
    }
}
