using Amazon.Core.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Contracts
{
    public interface IItemHistoryService
    {
        void AddRecord(long itemId,
            string fieldName,
            object fromValue,
            object toValue,
            long? by);

        void AddRecord(long itemId,
            string fieldName,
            object fromValue,
            string extendFromValue,
            object toValue,
            string extendToValue,
            long? by);

        void LogListingPrice(PriceChangeSourceType type,
                long listingId,
                string sku,
                decimal newPrice,
                decimal? oldPrice,
                DateTime when,
                long? by);
    }
}
