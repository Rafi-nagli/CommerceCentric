using System;
using System.Collections.Generic;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Sizes;
using Amazon.DTO;
using Amazon.DTO.Listings;

namespace Amazon.Core.Contracts.Db
{
    public interface IQuantityHistoryRepository : IRepository<QuantityHistory>
    {
        IList<QuantityHistoryDTO> GetByOrderId(string orderId);
        SoldSizeInfo GetSoldQtyByStyleIdAndSizeFromDate(string styleString, string size, DateTime? fromDate);
    }
}
