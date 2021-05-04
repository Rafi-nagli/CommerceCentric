using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models;
using Amazon.Core.Models.Search;
using Amazon.Core.Models.Settings;
using Amazon.Core.Models.Stamps;
using Amazon.DTO;
using Amazon.DTO.Orders;
using Amazon.Model.Implementation.Markets;
using Amazon.Web.Models;
using Amazon.Web.ViewModels.Mailing;

namespace Amazon.Web.ViewModels.Orders
{
    public class OrderQuickSummaryViewModel
    {
        public long? OrderEntityId { get; set; }
        public string OrderID { get; set; }
        public MarketType Market { get; set; }
        public string MarketplaceId { get; set; }
        public DateTime? OrderDate { get; set; }
        public bool OrderIsOnHold { get; set; }

        public string OrderStatus { get; set; }
        public bool OrderIsCancelled
        {
            get { return OrderStatus == OrderStatusEnumEx.Canceled; }
        }

        public AddressDTO ToAddress { get; set; }

        public int? WeightLb { get; set; }
        public double? WeightOz { get; set; }

        public decimal ActualShippingCost { get; set; }
        public string PriceCurrency { get; set; }

        public IList<LabelViewModel> TrackingNumbers { get; set; }

        public DateTime? ExpectedShipDate { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }

        //public string ShippingMethodName { get; set; }
        //public string ShippingProviderName { get; set; }
        //public DateTime? ShippingDate { get; set; }

        //public string DeliveryStatus { get; set; }
        //public DateTime? DeliveryDate { get; set; }

        public IList<OrderItemViewModel> Items { get; set; }
        public IList<RefundViewModel> Refunds { get; set; }

        public DateTime? AlignedExpectedShipDate
        {
            get
            {
                return ShippingUtils.AlignMarketDateByEstDayEnd(ExpectedShipDate, (MarketType)Market);
            }
        }

        public DateTime? AlignedExpectedDeliveryDate
        {
            get
            {
                return ShippingUtils.AlignMarketDateByEstDayEnd(ExpectedDeliveryDate, (MarketType)Market);
            }
        }


        public string OrderUrl
        {
            get { return UrlHelper.GetOrderUrl(OrderID); }
        }

        public string MarketName
        {
            get { return MarketHelper.GetMarketName((int) Market, MarketplaceId); }
        }

        public OrderQuickSummaryViewModel()
        {
            
        }

        public static OrderQuickSummaryViewModel GetByOrderId(IUnitOfWork db,
            ILogService log,
            IWeightService weightService,
            string orderId)
        {
            DTOOrder order = null;

            if (!String.IsNullOrEmpty(orderId))
            {
                orderId = orderId.RemoveWhitespaces();
                var orderNumber = OrderHelper.RemoveOrderNumberFormat(orderId);
                var filter = new OrderSearchFilter()
                {
                    EqualOrderNumber = orderNumber,
                    IncludeMailInfos = true,
                    IncludeNotify = false,

                    UnmaskReferenceStyles = false,
                };

                order = db.ItemOrderMappings
                    .GetFilteredOrdersWithItems(weightService, filter)
                    .FirstOrDefault();
            }

            if (order == null)
            {
                return null;
            }
            else
            {
                var refundRequestList = RefundViewModel.GetByOrderId(db, order.OrderId);

                var shippingPrice = 0M;

                var activeShippings = order.ShippingInfos.Where(i => i.IsActive).ToList();
                activeShippings.AddRange(order.MailInfos);
                activeShippings = activeShippings.OrderBy(sh => sh.LabelPurchaseDate).ToList();
                //if (order.MailInfos.Any(m => !m.LabelCanceled)
                //    && order.ShippingInfos.All(sh => String.IsNullOrEmpty(sh.LabelPath) || sh.LabelCanceled))
                //{
                //    var mailShipping = order.MailInfos.OrderBy(m => m.LabelPurchaseDate).FirstOrDefault(l => !l.LabelCanceled);
                //    activeShippings = new List<OrderShippingInfoDTO>() { mailShipping };
                //}
                //var mainActiveShipping = activeShippings.FirstOrDefault();
                //var shipmentProviderType = mainActiveShipping != null ? mainActiveShipping.ShipmentProviderType : (int)Core.Models.Settings.ShipmentProviderType.Stamps;
                var address = order.GetAddressDto();

                return new OrderQuickSummaryViewModel()
                {
                    OrderID = order.OrderId,
                    OrderEntityId = order.Id,
                    OrderDate = order.OrderDate,
                    OrderStatus = order.OrderStatus,
                    Market = (MarketType) order.Market,
                    MarketplaceId = order.MarketplaceId,

                    OrderIsOnHold = order.OnHold,

                    WeightLb = (int) Math.Floor(order.WeightD/16),
                    WeightOz = order.WeightD%16,

                    TrackingNumbers = activeShippings.Where(t => !String.IsNullOrEmpty(t.TrackingNumber)).Select(t => new LabelViewModel()
                    {
                        TrackingNumber = t.TrackingNumber,
                        Carrier = t.ShippingMethod.CarrierName,
                        FromType = (LabelFromType)t.LabelFromType,
                        TrackingStatusSource = t.TrackingStateSource,
                        ShippingDate = t.ShippingDate,
                        EstDeliveryDate = t.EstimatedDeliveryDate,
                        ActualDeliveryDate = t.ActualDeliveryDate,
                        LastTrackingStateUpdateDate = t.LastTrackingRequestDate,
                        IsCanceled = t.CancelLabelRequested || t.LabelCanceled,
                        DeliveryStatusMessage = GetDeliveryStatus(order.LatestDeliveryDate,
                            t.ActualDeliveryDate,
                            t.TrackingStateDate,
                            t.DeliveredStatus == (int)DeliveredStatusEnum.DeliveredToSender),
                        ShippingMethodName = t.ShippingMethod != null
                            ? ShippingUtils.PrepareMethodNameToDisplay(t.ShippingMethod.Name, t.DeliveryDaysInfo) : string.Empty,
                        ShippingProviderName = ShipmentProviderHelper.GetName((ShipmentProviderType)t.ShipmentProviderType),
                    }).ToList(),

                    PriceCurrency = PriceHelper.FormatCurrency(order.TotalPriceCurrency),

                    ExpectedShipDate = order.LatestShipDate,
                    ExpectedDeliveryDate = order.LatestDeliveryDate,

                    //ShippingMethodName = mainActiveShipping != null && mainActiveShipping.ShippingMethod != null
                    //    ? ShippingUtils.PrepareMethodNameToDisplay(mainActiveShipping.ShippingMethod.Name, mainActiveShipping.DeliveryDaysInfo) : string.Empty,
                    //ShippingProviderName = ShipmentProviderHelper.GetName((ShipmentProviderType)shipmentProviderType),
                    //ShippingDate = activeShippings.Where(i => i.IsActive).Max(i => i.ShippingDate),

                    //DeliveryDate = mainActiveShipping != null ? mainActiveShipping.ActualDeliveryDate : null,
                    //DeliveryStatus = GetDeliveryStatus(order.LatestDeliveryDate,
                    //    mainActiveShipping != null ? mainActiveShipping.ActualDeliveryDate : null,
                    //    mainActiveShipping != null ? mainActiveShipping.TrackingStateDate : null,
                    //    mainActiveShipping != null ? mainActiveShipping.DeliveredStatus == (int)DeliveredStatusEnum.DeliveredToSender : false),

                    Refunds = refundRequestList,

                    Items = order.Items.Select(i => new OrderItemViewModel(i, false, false)).ToList(),

                    ToAddress = address,
                };
            }
        }

        public static string GetDeliveryStatus(DateTime? expDeliveryDate,
            DateTime? deliveryDate,
            DateTime? lastStatusDate,
            bool isReturnedBack)
        {
            if (deliveryDate.HasValue)
            {
                if (!expDeliveryDate.HasValue 
                    || deliveryDate <= expDeliveryDate)
                    return "Order was delivered on time";
                return "Order was delivered late";
            }
            if (isReturnedBack)
            {
                return "Order returned back to us";
            }

            //var currentDate = DateTime.UtcNow;
            if (lastStatusDate.HasValue) //NOTE: removed, example CA orders have huge gap between status updates. && lastStatusDate.Value >= currentDate.AddDays(-3))
            {
                if (expDeliveryDate.HasValue
                    && lastStatusDate >= expDeliveryDate)
                {
                    return "Order in transit - late";
                }
                return "Order in transit";
            }

            return "n/a";
        }
    }
}