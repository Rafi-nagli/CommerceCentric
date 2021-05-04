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
    public class FeedbackBlackListRepository : Repository<FeedbackBlackList>, IFeedbackBlackListRepository
    {
        public FeedbackBlackListRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IList<FeedbackBlackListDto> GetAllExtendedAsDto()
        {
            return AsDto(unitOfWork.GetSet<ViewFeedbackBlackList>()).ToList();
        }

        public FeedbackBlackListDto GetExtendedByIdAsDto(long id)
        {
            return AsDto(unitOfWork.GetSet<ViewFeedbackBlackList>().Where(bl => bl.Id == id)).FirstOrDefault();
        }

        public IList<FeedbackBlackListDto> GetSimular(DTOOrder order)
        {
            var query = from b in unitOfWork.GetSet<ViewFeedbackBlackList>()
                where b.PersonName == order.PersonName
                      || b.BuyerName == order.BuyerName
                      || (b.ShippingCountry == order.ShippingCountry
                          && b.ShippingCity == order.ShippingCity
                          && b.ShippingState == order.ShippingState
                          && b.ShippingAddress1 == order.ShippingAddress1
                          && b.ShippingAddress2 == order.ShippingAddress2)
                      || (b.ShippingZip == order.ShippingZip &&
                          b.ShippingZipAddon == order.ShippingZipAddon)
                select b;
            return AsDto(query).ToList();
        }

        private IQueryable<FeedbackBlackListDto> AsDto(IQueryable<ViewFeedbackBlackList> query)
        {
            return query.Select(i => new FeedbackBlackListDto()
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
