using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Entities;
using Amazon.Core.Models;
using Amazon.DTO;

namespace Amazon.Core.Contracts.Db
{
    public interface IMailLabelInfoRepository : IRepository<MailLabelInfo>
    {
        void StoreInfo(MailLabelDTO mailInfo, IList<DTOOrderItem> mailItems, DateTime when, long? by);
        IEnumerable<ShippingDTO> GetInfosToFulfillAsDTO(MarketType market,
            string marketplaceId);

        IEnumerable<OrderShippingInfoDTO> GetByOrderIdsAsDto(IList<long> orderIds);
        IQueryable<OrderShippingInfoDTO> GetAllAsDto();
    }
}
