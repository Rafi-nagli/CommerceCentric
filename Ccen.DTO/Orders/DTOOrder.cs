using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.DTO.Contracts;
using Amazon.DTO.Orders;

namespace Amazon.DTO
{
    public class DTOOrder : DTOMarketOrder, ISortableEntity
    {
        public IList<long> ItemStyleIdList
        {
            get
            {
                return Items != null ?
                    Items.Where(i => i.StyleId.HasValue).Select(i => i.StyleId ?? 0).ToList()
                    : new List<long>();
            }
        }


        #region ISortableByLocation

        public int SortIsle
        {
            get
            {
                var firstItem = Items != null && Items.Any() ? Items.First() : null;
                if (firstItem != null)
                {
                    return firstItem.Locations != null && firstItem.Locations.Count > 0
                        ? firstItem.Locations.OrderByDescending(i => i.IsDefault).First().SortIsle
                        : int.MaxValue;
                }
                return int.MaxValue;
            }
        }

        public int SortSection
        {
            get
            {
                var firstItem = Items != null && Items.Any() ? Items.First() : null;
                if (firstItem != null)
                {
                    return firstItem.Locations != null && firstItem.Locations.Count > 0
                        ? firstItem.Locations.OrderByDescending(i => i.IsDefault).First().SortSection
                        : int.MaxValue;
                }
                return int.MaxValue;
            }
        }

        public int SortShelf
        {
            get
            {
                var firstItem = Items != null && Items.Any() ? Items.First() : null;
                if (firstItem != null)
                {
                    return firstItem.Locations != null && firstItem.Locations.Count > 0
                        ? firstItem.Locations.OrderByDescending(i => i.IsDefault).First().SortShelf
                        : int.MaxValue;
                }
                return int.MaxValue;
            }
        }

        public string SortSize
        {
            get
            {
                return (Items != null && Items.Count > 0) ? Items[0].StyleSize : "";
            }
        }

        public string SortColor
        {
            get
            {
                return (Items != null && Items.Count > 0) ? Items[0].StyleColor : "";
            }
        }

        public string FirstItemName
        {
            get { return (Items != null && Items.Count > 0) ? Items[0].Title : ""; }
        }

        public string SortStyleString
        {
            get { return (Items != null && Items.Count > 0) ? Items[0].StyleID : ""; }
        }

        public string SortOrderId
        {
            get { return OrderId; }
        }

        public int ShippingMethodId
        {
            get
            {
                if (ShippingInfos != null && ShippingInfos.Any(sh => sh.IsActive))
                {
                    var methodId = ShippingInfos.Where(sh => sh.IsActive).Max(sh => (int?)sh.ShippingMethodId);
                    return methodId ?? 0;
                }
                return 0;
            }
        }
        #endregion


        //Navigations
        public IList<OrderShippingInfoDTO> ShippingInfos { get; set; }
        public IList<OrderShippingInfoDTO> MailInfos { get; set; }

        public IList<ListingOrderDTO> Items { get; set; }
        public IList<ListingOrderDTO> SourceItems { get; set; }

    }


}
