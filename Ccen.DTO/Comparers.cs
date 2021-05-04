using System.Collections.Generic;
using System.Linq;

namespace Amazon.DTO 
{
    public class DTOOrderComparer : IEqualityComparer<DTOOrder>
    {
        public bool Equals(DTOOrder x, DTOOrder y)
        {
            return x.OrderId == y.OrderId && x.DropShipperId == y.DropShipperId && x.SubOrderNumber == y.SubOrderNumber;
        }

        public int GetHashCode(DTOOrder obj)
        {
            return (obj.OrderId + "_" + obj.DropShipperId + "_" + obj.SubOrderNumber).GetHashCode();
        }
    }

    public class DTOOrderItemComparer : IEqualityComparer<DTOOrder>
    {
        public bool Equals(DTOOrder x, DTOOrder y)
        {
            return x.Id == y.Id
                && x.Items.First().ItemOrderId == y.Items.First().ItemOrderId
                && x.Items.First().StyleItemId == y.Items.First().StyleItemId;
        }

        public int GetHashCode(DTOOrder obj)
        {
            return (obj.Items.First().ItemOrderId + "_" + (obj.Items.First().StyleItemId.ToString() ?? "")).GetHashCode();
        }
    }


    public class DTOShippingInfoComparer : IEqualityComparer<DTOOrder>
    {
        public bool Equals(DTOOrder x, DTOOrder y)
        {
            return x.ShippingInfos.First().Id == y.ShippingInfos.First().Id;
        }

        public int GetHashCode(DTOOrder obj)
        {
            return obj.ShippingInfos.First().Id.GetHashCode();
        }
    }





    public class DTOListingOrderComparer : IEqualityComparer<ListingOrderDTO>
    {
        public bool Equals(ListingOrderDTO x, ListingOrderDTO y)
        {
            return x.ShippingInfoId == y.ShippingInfoId && x.ItemOrderId == y.ItemOrderId && x.StyleId == y.StyleId;
        }

        public int GetHashCode(ListingOrderDTO obj)
        {
            return (obj.ShippingInfoId + "_" + obj.ItemOrderId + "_" + obj.StyleId).GetHashCode();
        }
    }


}
