using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models.Histories.HistoryDatas;
using Amazon.Web.ViewModels.Inventory.SizeHistories;

namespace Amazon.Web.ViewModels.Inventory
{
    public class StyleSizeActionHistoryRecordViewModel : IHistoryRecord
    {
        public long Id { get; set; }

        public string ActionName { get; set; }
        public string Data { get; set; }

        public int? Price { get; set; }
        public int? BeforePrice { get; set; }

        public DateTime UpdateDate { get; set; }
        public long? UserId { get; set; }
        public string UserName { get; set; }


        public string Reason
        {
            get
            {
                if (ActionName == "AddPermancentSale")
                    return "Price Change";
                if (ActionName == "RemoveSale")
                    return "Remove Sale";
                if (ActionName == "AddSale")
                    return "Add Sale";

                return ActionName;
            }
        }

        public string EntityType { get { return "Style Size"; } }
        public string EntityName { get { return ""; } }

        public string FromValue { get; set; }
        public string ToValue { get; set; }

        public DateTime When { get { return UpdateDate; } }
        public string ByName { get { return UserName; } }

        public void Prepare()
        {
            var model = JsonHelper.Deserialize<HistorySaleData>(Data);
            ToValue = PriceHelper.PriceToString(model.SalePrice);
        }


        public static IQueryable<IHistoryRecord> GetRecords(IUnitOfWork db, 
            long styleItemId,
            string[] actionNames)
        {
            var query = from h in db.StyleItemActionHistories.GetAll()
                        join u in db.Users.GetAll() on h.CreatedBy equals u.Id into withUser
                        from u in withUser.DefaultIfEmpty()
                        where h.StyleItemId == styleItemId
                            && actionNames.Contains(h.ActionName)
                        orderby h.CreateDate descending
                        select new StyleSizeActionHistoryRecordViewModel()
                        {
                            Id = h.Id,
                            ActionName = h.ActionName,
                            Data = h.Data,
                            UpdateDate = h.CreateDate,
                            UserId = h.CreatedBy,
                            UserName = u.Name
                        };

            return query;
        }
    }
}