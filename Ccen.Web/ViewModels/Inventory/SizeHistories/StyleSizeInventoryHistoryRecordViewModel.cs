using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Core;
using Amazon.Core.Entities.Enums;

namespace Amazon.Web.ViewModels.Inventory
{
    public class StyleSizeInventoryHistoryRecordViewModel
    {
        public long Id { get; set; }
        public int? NewQuantity { get; set; }
        public int? OldQuantity { get; set; }

        public int? Remaining { get; set; }
        public int? BeforeRemaining { get; set; }

        public DateTime? UpdateDate { get; set; }
        public long? UserId { get; set; }
        public string UserName { get; set; }

        public string Tag { get; set; }
        public string SourceEntityName { get; set; }
        public int Type { get; set; }
        public string FormattedType
        {
            get
            {
                return QuantityChangeSourceTypeHelper.GetName((QuantityChangeSourceType)Type);
            }
        }

        public static IQueryable<StyleSizeInventoryHistoryRecordViewModel> GetRecords(IUnitOfWork db, 
            long styleItemId,
            bool includeSnapshoot)
        {
            var query = from h in db.StyleItemQuantityHistories.GetAll()
                        join u in db.Users.GetAll() on h.CreatedBy equals u.Id into withUser
                        from u in withUser.DefaultIfEmpty()
                        where h.StyleItemId == styleItemId
                        orderby h.CreateDate descending 
                        select new StyleSizeInventoryHistoryRecordViewModel()
                        {
                            Id = h.Id,
                            NewQuantity = h.Quantity,
                            OldQuantity = h.FromQuantity,
                            Remaining = h.RemainingQuantity,
                            BeforeRemaining = h.BeforeRemainingQuantity,
                            UpdateDate = h.CreateDate,
                            Type = h.Type,                            
                            Tag = h.Tag,
                            SourceEntityName = h.SourceEntityName,
                            UserId = h.CreatedBy,
                            UserName = u.Name
                        };

            if (!includeSnapshoot)
            {
                query = query.Where(h => h.Type != (int) QuantityChangeSourceType.RemainingChanged);
            }

            return query;
        }
    }
}