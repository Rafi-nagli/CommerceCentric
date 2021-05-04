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
using Amazon.DTO;
using Amazon.Web.Models;

namespace Amazon.Web.ViewModels.Orders
{
    public class OrderInfoViewModel
    {
        public long Id { get; set; }
        public long EntityId { get; set; }
        public string OrderId { get; set; }

        public int Market { get; set; }
        public string MarketplaceId { get; set; }

        public bool OnHold { get; set; }

        public bool HasPhoneNumber { get; set; }

        //For overdue shipDate
        public DateTime? ExpectedShipDate { get; set; }
        public DateTime? AlignedExpectedShipDate
        {
            get
            {
                return ShippingUtils.AlignMarketDateByEstDayEnd(ExpectedShipDate, (MarketType)Market);
            }
        }
        public string OrderStatus { get; set; }

        //For purchase message
        public bool HasLabel { get; set; }
        public bool HasAllLabels { get; set; }
        public bool HasMailLabels { get; set; }
        //NOTE: Only with print errors
        public IList<LabelViewModel> Labels { get; set; } 

        //For address validation
        public int AddressValidationStatus { get; set; }
        public bool IsDismissAddressValidation { get; set; }
        public bool HasComment { get; set; }


        public string InitialServiceType { get; set; }

        //For calculating total price
        public int ShipmentProviderType { get; set; }
        public decimal StampsShippingCost { get; set; } 

        //For check by style
        //TODO: now temporary disabled
        public int NumberByLocation { get; set; }

        //For w/o packages
        public int Selected { get; set; }
        public int? ShippingMethodId { get; set; }

        //For display notifications
        public IList<OrderNotifyViewModel> Notifies { get; set; }

        public OrderInfoViewModel()
        {
            
        }

        public OrderInfoViewModel(DTOOrder order)
        {
            Id = order.Id;
            EntityId = order.Id;
            OrderId = order.OrderId;
            Market = order.Market;
            MarketplaceId = order.MarketplaceId;

            InitialServiceType = order.InitialServiceType;

            OnHold = order.OnHold;

            ExpectedShipDate = order.LatestShipDate;
            OrderStatus = order.OrderStatus;

            var activeShipping = order.ShippingInfos.FirstOrDefault(sh => sh.IsActive);
            
            HasLabel = activeShipping != null 
                && order.ShippingInfos.Where(sh => sh.IsActive).Any(sh => sh.LabelPurchaseResult == (int)LabelPurchaseResultType.Success);
            HasAllLabels = activeShipping != null 
                && order.ShippingInfos.Where(sh => sh.IsActive).All(sh => sh.LabelPurchaseResult == (int)LabelPurchaseResultType.Success);
            HasMailLabels = order.MailInfos != null && order.MailInfos.Any();

            //NOTE: all only labels with errors
            Labels = order.ShippingInfos.Where(sh => sh.LabelPurchaseResult == (int) LabelPurchaseResultType.Error)
                .Select(sh => new LabelViewModel()
                {
                    PurchaseMessage = sh.LabelPurchaseMessage,
                    PurchaseResult = sh.LabelPurchaseResult
                }).ToList();

            IsDismissAddressValidation = order.IsDismissAddressValidation;
            AddressValidationStatus = order.AddressValidationStatus;
            HasComment = order.LastCommentDate.HasValue;
            
            Selected = activeShipping != null ? activeShipping.ShippingGroupId : 0;
            ShippingMethodId = activeShipping != null ? activeShipping.ShippingMethod.Id : (int?)null;

            ShipmentProviderType = order.ShipmentProviderType;
            StampsShippingCost = order.ShippingInfos
                .Where(sh => sh.IsActive)
                .Sum(sh => sh.StampsShippingCost ?? 0);
            
            Notifies = order.Notifies.Select(n => new OrderNotifyViewModel(n)).ToList();
        }


        public static IList<OrderInfoViewModel> GetAll(IUnitOfWork db, 
            ILogService log,
            IWeightService weightService,
            int? batchId)
        {
            var filter = new OrderSearchFilter();
            if (!batchId.HasValue)
            {
                filter.OrderStatus = OrderStatusEnumEx.AllUnshipped;
            }
            else
            {
                filter.OrderStatus = OrderStatusEnumEx.AllUnshippedWithShipped;
                filter.BatchId = batchId.Value;
            }
            filter.IncludeNotify = true;
            filter.IncludeMailInfos = true;

            IList<DTOOrder> orders = db.ItemOrderMappings
                .GetFilteredOrderInfos(weightService, filter)
                .ToList();

            return orders.Select(o => new OrderInfoViewModel(o))
                .Where(o => !o.HasAllLabels && !o.HasMailLabels)
                .ToList();
        } 
    }
}