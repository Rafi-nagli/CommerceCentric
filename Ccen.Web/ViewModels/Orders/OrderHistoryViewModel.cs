using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Validation;
using Amazon.Core.Entities;
using Amazon.Core.Helpers;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.Core.Models.Search;
using Amazon.Core.Models.Stamps;
using Amazon.Core.Models.SystemActions.SystemActionDatas;
using Amazon.Core.Views;
using Amazon.DTO;
using Amazon.DTO.Contracts;
using Amazon.DTO.Inventory;
using Amazon.DTO.Orders;
using Amazon.Model.Implementation;
using Amazon.Web.Models;
using Amazon.Web.ViewModels.Emails;
using Amazon.Web.ViewModels.Inventory;
using Amazon.Web.ViewModels.Mailing;
using Amazon.Web.ViewModels.Orders;
using Amazon.Web.ViewModels.Products;
using Amazon.Web.ViewModels.Results;
using Newtonsoft.Json;
using Amazon.DTO.Users;
using UrlHelper = Amazon.Web.Models.UrlHelper;


namespace Amazon.Web.ViewModels
{
    public class OrderHistoryViewModel
    {
        public long? OrderEntityId { get; set; }
        public string OrderID { get; set; }
        public MarketType Market { get; set; }
        public string MarketplaceId { get; set; }
        public DateTime? OrderDate { get; set; }

        public int? WeightLb { get; set; }

        public double? WeightOz { get; set; }

        public decimal TotalPrice { get; set; }

        public decimal ActualShippingCost { get; set; }

        public string PriceCurrency { get; set; }
        
        public AddressViewModel ToAddress { get; set; }
        public IList<OrderItemViewModel> Items { get; set; }

        public IList<OrderChangeViewModel> Changes { get; set; }

        public IList<OrderLinkViewModel> AnotherOrders { get; set; }

        public string MarketReturnUrl
        {
            get { return UrlHelper.GetSellarCentralReturnUrl(Market, OrderID); }
        }

        public string MarketOrderUrl
        {
            get { return UrlHelper.GetSellerCentralOrderUrl(Market, MarketplaceId, OrderID); }
        }

        public string OrderUrl
        {
            get { return UrlHelper.GetOrderUrl(OrderID); }
        }

        public override string ToString()
        {
            var text = ToStringHelper.ToString(this);
            return text;
        }

        public OrderHistoryViewModel()
        {
            Changes = new List<OrderChangeViewModel>();
        }
        
        public static OrderHistoryViewModel GetByOrderId(IUnitOfWork db, 
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
                var anotherBuyerOrders = new List<OrderLinkViewModel>();
                if (!String.IsNullOrEmpty(order.BuyerEmail))
                {
                    anotherBuyerOrders = db.Orders.GetAll().Where(o => o.BuyerEmail == order.BuyerEmail)
                        .ToList()
                        .Where(o => o.Id != order.Id)
                        .OrderByDescending(o => o.OrderDate)
                        .Select(o => new OrderLinkViewModel()
                        {
                            OrderId = o.Id,
                            OrderNumber = o.AmazonIdentifier,
                            OrderDate = o.OrderDate,
                            OrderStatus = o.OrderStatus,
                            Market = o.Market,
                            MarketplaceId = o.MarketplaceId
                        })
                        .ToList();
                }

                var comments = db.OrderComments.GetByOrderIdDto(order.Id).ToList();

                var emails = db.Emails.GetAllWithOrder(new EmailSearchFilter() { OrderId = orderId }).ToList();

                var returnRequests = db.ReturnRequests.GetAll()
                    .OrderByDescending(r => r.CreateDate)
                    .Select(r => new ReturnRequestDTO()
                    {
                        OrderNumber = r.OrderNumber,
                        ReceiveDate = r.ReceiveDate,
                        ItemName = r.ItemName,
                        CustomerComments = r.CustomerComments,
                        Details = r.Details,
                        CreateDate = r.CreateDate,
                        Reason = r.Reason
                    })
                    .Where(r => r.OrderNumber == order.OrderId)
                    .ToList();

                var refundRequests = RefundViewModel.GetByOrderId(db, order.OrderId);

                var labels = order.ShippingInfos
                    .Where(i => !String.IsNullOrEmpty(i.LabelPath) 
                        //|| i.LabelPurchaseResult != null //NOTE: in case when bag system shows user issue (keeped twice shippings with labels)
                    )
                    .ToList();
                labels.AddRange(order.MailInfos);
                
                
                var address = order.GetAddressDto();

                var changes = db.OrderChangeHistories.GetByOrderIdDto(order.Id)
                    .ToList()
                    .OrderByDescending(ch => ch.ChangeDate)
                    .Select(ch => new OrderChangeViewModel(ch, emails))
                    .Where(ch => ch.ChangeType != OrderChangeTypes.None) //NOTE: Skipped empty
                    .ToList();
                
                changes.Add(OrderChangeViewModel.BuildCreateOrderChange(order));
                changes.AddRange(comments.Select(c => new OrderChangeViewModel(c)).ToList());
                changes.AddRange(emails.Select(e => new OrderChangeViewModel(e)).ToList());
                changes.AddRange(labels.SelectMany(l => OrderChangeViewModel.BuildChanges(l)).ToList());
                changes.AddRange(refundRequests.Select(r => new OrderChangeViewModel(r)).ToList());
                changes.AddRange(returnRequests.Select(r => new OrderChangeViewModel(r)).ToList());

                return new OrderHistoryViewModel
                {
                    //Notes =  string.Format("{0} {1}", order.OrderId, itemsNotes),
                    OrderID = order.OrderId,
                    OrderEntityId = order.Id,
                    OrderDate = order.OrderDate,
                    Market = (MarketType)order.Market,
                    MarketplaceId = order.MarketplaceId,

                    WeightLb = (int)Math.Floor(order.WeightD / 16),
                    WeightOz = order.WeightD % 16,
                    TotalPrice = order.TotalPrice,
                    PriceCurrency = PriceHelper.FormatCurrency(order.TotalPriceCurrency),
                    
                    Items = order.Items.Select(i => new OrderItemViewModel(i, false, false)).ToList(),
                    Changes = changes.OrderByDescending(c => c.ChangeDate).ToList(),

                    AnotherOrders = anotherBuyerOrders,

                    ToAddress = new AddressViewModel(address),
                };
            }
        }
    }
}