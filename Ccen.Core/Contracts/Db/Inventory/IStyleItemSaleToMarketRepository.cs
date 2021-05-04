using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Entities.Inventory;
using Amazon.DTO.Inventory;

namespace Amazon.Core.Contracts.Db.Inventory
{
    public interface IStyleItemSaleToMarketRepository : IRepository<StyleItemSaleToMarket>
    {
        IQueryable<StyleItemSaleToMarketDTO> GetAllAsDto();

        void UpdateForSale(long saleId,
            IList<StyleItemSaleToMarketDTO> saleToMarkets,
            DateTime when,
            long? by);
    }
}
