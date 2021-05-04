using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Entities;
using Amazon.Core.Entities.VendorOrders;
using Amazon.DTO.Vendors;

namespace Amazon.Core.Contracts.Db
{
    public interface IVendorOrderItemSizeRepository : IRepository<VendorOrderItemSize>
    {
        IQueryable<VendorOrderItemSizeDTO> GetAllAsDto();

        void UpdateSizesForVendorItem(long vendorItemId,
            IList<VendorOrderItemSizeDTO> sizes,
            DateTime when,
            long? by);
    }
}
