using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Web.Mvc;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models;
using Amazon.Core.Models.Settings;
using Amazon.DTO;
using Amazon.Model.General;
using Amazon.Model.Implementation;
using Amazon.Model.Implementation.Addresses;
using Amazon.Model.Implementation.Errors;
using Amazon.Model.Implementation.Markets;
using Amazon.Model.Implementation.Markets.Amazon;
using Amazon.Model.Implementation.Sync;
using Amazon.Model.Implementation.Validation;
using Amazon.Web.Filters;
using Amazon.Web.Models;
using Amazon.Web.ViewModels;
using Amazon.Web.ViewModels.Grid;
using Amazon.Web.ViewModels.Html;
using Amazon.Web.ViewModels.Inventory;
using Amazon.Web.ViewModels.Messages;
using Amazon.Web.ViewModels.Orders;
using Amazon.Web.ViewModels.Pages;

using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using UrlHelper = Amazon.Web.Models.UrlHelper;

namespace Amazon.Web.Controllers
{
    [Authorize(Roles = AccessManager.AllBaseRole)]
    public partial class OrderByPageController : BaseController
    {
        public override string TAG
        {
            get { return "OrderByPageController."; }
        }

        public virtual ActionResult Orders(string orderId)
        {
            LogI("Orders");

            return View(new OrderPageViewModel
            {
                DefaultMarket = MarketHelper.DefaultUIMarket,
                DefaultMarketplaceId = MarketHelper.DefaultUIMarketplaceId,
                DefaultDropShipperId = DSHelper.DefaultPAId,
                SearchOrderId = orderId,
            });
        }


        [Compress]
        public virtual ActionResult GetOrders(GridRequest request,
            int? market,
            string marketplaceId,
            string orderStatus,
            string shippingStatus,
            string dateFrom,
            string dateTo,
            string buyerName,
            string orderNumber,
            long? batchId,
            long? dropShipperId,
            string styleId,
            long? styleItemId)
        {
            LogI("Begin GetOrders, market=" + market
                + ", marketplaceId=" + marketplaceId
                + ", orderStatus=" + orderStatus
                + ", shipppingStatus=" + shippingStatus
                + ", DateFrom=" + dateFrom
                + ", DateTo=" + dateTo
                + ", BuyerName=" + buyerName
                + ", OrderNumber=" + orderNumber
                + ", BatchId=" + batchId
                + ", dropShipperId=" + dropShipperId
                + ", StyleId=" + styleId
                + ", StyleItemId=" + styleItemId);

            bool excludeWithLabels = false;
            bool includeForceVisible = false;
            string[] orderStatusList = null;
            if (!String.IsNullOrEmpty(orderStatus))
                orderStatusList = new[] { orderStatus };
            //Exclude canceled from batch on UI


            if (batchId == 0)
                batchId = null;

            if (orderStatus == null)
            {
                if (batchId != null)
                {
                    orderStatusList = OrderStatusEnumEx.AllUnshippedWithShipped;
                }
                else
                {
                    orderStatusList = OrderStatusEnumEx.AllUnshipped;
                    excludeWithLabels = true;
                    includeForceVisible = true;
                }
            }

            buyerName = StringHelper.TrimWhitespace(buyerName);
            orderNumber = StringHelper.TrimWhitespace(orderNumber);
            styleId = StringHelper.TrimWhitespace(styleId);

            var pageSize = request.ItemsPerPage;
            var model = new OrderSearchFilterViewModel
            {
                //FulfillmentChannel = FulfillmentChannelTypeEx.MFN,
                Market = (MarketType)(market ?? (int)MarketHelper.DefaultUIMarket),
                MarketplaceId = marketplaceId ?? MarketHelper.DefaultUIMarketplaceId,
                DropShipperId = dropShipperId,
                OrderStatus = orderStatusList,
                ShippingStatus = shippingStatus,
                ExcludeWithLabels = excludeWithLabels,
                IncludeForceVisible = includeForceVisible,
                DateFrom = dateFrom.HasValue()
                    ? (DateTime?)
                        DateTime.ParseExact(dateFrom, "MM/dd/yyyy", new CultureInfo("en-US"), DateTimeStyles.None)
                    : null,
                DateTo = dateTo.HasValue()
                    ? (DateTime?)
                        DateTime.ParseExact(dateTo, "MM/dd/yyyy", new CultureInfo("en-US"), DateTimeStyles.None)
                    : null,
                BuyerName = string.IsNullOrEmpty(buyerName) ? null : buyerName.Trim(),
                BatchId = batchId,
                StyleId = string.IsNullOrEmpty(styleId) ? null : styleId.Trim(),
                StyleItemId = styleItemId,

                StartIndex = (request.Page - 1) * pageSize,
                LimitCount = pageSize,
                SortField = request.SortField,
                SortMode = request.SortMode == "asc" ? 0 : 1,
            };

            if (!string.IsNullOrEmpty(orderNumber))
            {
                //model.EqualOrderNumber = orderNumber;
                if (orderNumber.Contains("-"))
                    model.OrderNumber = orderNumber.Trim();
                else
                    model.OrderNumber = OrderHelper.FormatDisplayOrderNumber(orderNumber, MarketType.None);
            }

            var searchResult = OrderViewModel.GetFilteredForDisplay(ReadDb,
                LogService,
                WeightService,
                model,
                AccessManager.IsFulfilment,
                SortMode.ByShippingMethodThenLocation);

            for (int i = 0; i < searchResult.Items.Count(); i++)
            {
                searchResult.Items[i].NumberByLocation = i;
            }

            if (!String.IsNullOrEmpty(model.OrderNumber))
            {
                OrderPageViewModel.AddSearchHistory(Db,
                    model.OrderNumber,
                    Time.GetUtcTime(),
                    AccessManager.UserId);
            }

            LogI("End GetOrders");
            var data = new GridResponse<OrderViewModel>(searchResult.Items, searchResult.Items.Count);
            data.RequestTimeStamp = request.TimeStamp;
            return JsonGet(data);
        }

    }
}
