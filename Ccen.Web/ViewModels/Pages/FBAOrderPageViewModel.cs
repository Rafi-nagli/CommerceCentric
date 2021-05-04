using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models;
using Amazon.Web.Models;

namespace Amazon.Web.ViewModels.Pages
{
    public class FBAOrderPageViewModel
    {
        public int PreviousDayOrdersCount { get; set; }
        public MarketType Market { get; set; }

        public void Init(IUnitOfWork db)
        {
            var now = DateHelper.GetAppNowTime();
            DateTime fromDate;
            DateTime toDate;
            if (now.Hour > 10)
            {
                fromDate = now.Date.AddDays(-1).AddHours(10);
                toDate = fromDate.AddHours(24);
            }
            else
            {
                fromDate = now.Date.AddDays(-2).AddHours(10);
                toDate = fromDate.AddHours(24);
            }

            PreviousDayOrdersCount = OrderViewModel.GetFilteredForDisplayCount(db, 
                new OrderSearchFilterViewModel()
                {
                    Market = Market,
                    FulfillmentChannel = FulfillmentChannelTypeEx.AFN,
                    OrderStatus = OrderStatusEnumEx.AllUnshippedWithShipped,
                    DateFrom = fromDate,
                    DateTo = toDate
                },
                false);
        }
    }
}