using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Entities.Sizes;
using Amazon.Core.Views;
using Amazon.Core;
using Amazon.DTO;

namespace Amazon.DAL.Repositories
{
    public class BuyerBlackListRepository : Repository<BuyerBlackList>, IBuyerBlackListRepository
    {
        public BuyerBlackListRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IList<BuyerBlackListDto> GetAllExtendedAsDto()
        {
            return AsDto(unitOfWork.GetSet<ViewBuyerBlackList>()).ToList();
        }

        public BuyerBlackListDto GetExtendedByIdAsDto(long id)
        {
            return AsDto(unitOfWork.GetSet<ViewBuyerBlackList>().Where(bl => bl.Id == id)).FirstOrDefault();
        }

        public IList<BuyerBlackListDto> GetSimular(DTOMarketOrder order)
        {
            var query = from b in unitOfWork.GetSet<ViewBuyerBlackList>()
                where b.BuyerEmail == order.BuyerEmail
                select b;
            return AsDto(query).ToList();
        }

        private IQueryable<BuyerBlackListDto> AsDto(IQueryable<ViewBuyerBlackList> query)
        {
            return query.Select(i => new BuyerBlackListDto()
            {
                Id = i.Id,
                OrderId = i.OrderId,
                MarketOrderId = i.MarketOrderId,
                Reason = i.Reason,
                CreateDate = i.CreateDate,

                Market = i.Market,
                MarketplaceId = i.MarketplaceId,
                OrderDate = i.OrderDate,
                BuyerName = i.BuyerName,
                BuyerEmail = i.BuyerEmail,
                PersonName = i.PersonName,
                AmazonEmail = i.AmazonEmail,

                ShippingCountry = i.ShippingCountry,
                ShippingAddress1 = i.ShippingAddress1,
                ShippingAddress2 = i.ShippingAddress2,
                ShippingCity = i.ShippingCity,
                ShippingState = i.ShippingState,
                ShippingZip = i.ShippingZip,
                ShippingZipAddon = i.ShippingZipAddon,
                ShippingPhone = i.ShippingPhone,
            });
        }
    }
}
