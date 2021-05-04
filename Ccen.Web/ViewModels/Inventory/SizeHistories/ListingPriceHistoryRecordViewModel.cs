using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models.Histories.HistoryDatas;
using Amazon.Model.Implementation.Markets;
using Amazon.Web.ViewModels.Inventory.SizeHistories;

namespace Amazon.Web.ViewModels.Inventory
{
    public class ListingPriceHistoryRecordViewModel : IHistoryRecord
    {
        public long Id { get; set; }

        public long? ListingId { get; set; }

        public string SKU { get; set; }

        public int Market { get; set; }
        public string MarketplaceId { get; set; }

        public int Type { get; set; }

        public decimal? Price { get; set; }
        public decimal? OldPrice { get; set; }

        public DateTime UpdateDate { get; set; }
        public long? UserId { get; set; }
        public string UserName { get; set; }


        public string Reason
        {
            get
            {
                return PriceChangeSourceTypeHelper.GetName((PriceChangeSourceType)Type);
            }
        }

        public string EntityType { get { return "Listing"; } }
        public string EntityName { get { return SKU + " (" + MarketHelper.GetDotShortName(Market, MarketplaceId) + ")"; } }

        public string FromValue { get { return PriceHelper.PriceToString(OldPrice); } }
        public string ToValue { get { return PriceHelper.PriceToString(Price); } }

        public DateTime When { get { return UpdateDate; } }
        public string ByName { get { return UserName; } }

        public void Prepare()
        {
            
        }


        public static IQueryable<IHistoryRecord> GetRecords(IUnitOfWork db, 
            long styleItemId)
        {
            var query = from h in db.PriceHistories.GetAll()
                        join u in db.Users.GetAll() on h.ChangedBy equals u.Id into withUser
                        from u in withUser.DefaultIfEmpty()
                        join l in db.Listings.GetAll() on h.ListingId equals l.Id
                        join i in db.Items.GetAll() on l.ItemId equals i.Id
                        where i.StyleItemId == styleItemId
                        orderby h.ChangeDate descending
                        select new ListingPriceHistoryRecordViewModel()
                        {
                            Id = h.Id,
                            SKU = h.SKU,                            
                            UpdateDate = h.ChangeDate,
                            UserId = h.ChangedBy,
                            UserName = u.Name,
                            ListingId = h.ListingId,
                            Market = l.Market,
                            MarketplaceId = l.MarketplaceId,
                            OldPrice = h.OldPrice,
                            Price = h.Price,
                            Type = h.Type
                        };

            return query;
        }
    }
}