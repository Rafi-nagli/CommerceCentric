using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Entities.Sizes;
using Amazon.Core.Views;
using Amazon.DTO;
using Amazon.DTO.ScanOrders;
using Amazon.DTO.Sizes;

namespace Amazon.Core.Contracts.Db
{
    public interface IScannedRepository : IRepository<ViewScannedSoldQuantity>
    {
        IQueryable<SoldSizeInfo> GetSentInStoreQuantities();
        IQueryable<SoldSizeInfo> GetSentToFBAQuantities();
        IQueryable<ScanOrderDTO> GetScanOrdersAsDto();
        IQueryable<ScanItemDTO> GetScanItemAsDto();
    }
}
