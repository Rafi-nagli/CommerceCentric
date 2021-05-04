using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Entities;
using Amazon.Core.Models;
using Amazon.DTO;

namespace Amazon.Core.Contracts.Db
{
    public interface IBuyBoxStatusRepository : IRepository<BuyBoxStatus>
    {
        IQueryable<BuyBoxStatusDTO> GetAllWithItems();

        void UpdateBulk(ILogService log,
            ITime time,
            IList<BuyBoxStatusDTO> buyBoxList,
            MarketType market,
            string marketplaceId);

        void UpdateBulkByBarcode(IList<BuyBoxStatusDTO> buyBoxList,
            MarketType market,
            string marketplaceId,
            DateTime when);

        void Update(ILogService log,
            ITime time,
            BuyBoxStatusDTO buyBoxInfo,
            MarketType market,
            string marketplaceId);
    }
}
