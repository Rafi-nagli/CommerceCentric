using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Core;
using Amazon.Core.Entities.Orders;
using Amazon.Core.Models;


namespace Amazon.Web.ViewModels.Pages
{
    public class OrderPageViewModel
    {
        public MarketType DefaultMarket { get; set; }
        public string DefaultMarketplaceId { get; set; }
        public long? DefaultDropShipperId { get; set; }

        public string SearchOrderId { get; set; }


        public static IList<string> GetLastOrderSearchList(IUnitOfWork db, long? by)
        {
            if (!by.HasValue)
                return new List<string>();

            var lastOrderSearches = db.OrderSearches
                .GetAllAsDto()
                .OrderByDescending(s => s.CreateDate)
                .Where(s => s.CreatedBy == by)
                .Select(s => s.OrderNumber)
                .Take(10);

            return lastOrderSearches.ToList();
        }

        public static void AddSearchHistory(IUnitOfWork db, 
            string orderNumber,
            DateTime when,
            long? by)
        {
            var lastOrderSearches = db.OrderSearches
                .GetAllAsDto()
                .OrderByDescending(s => s.CreateDate)
                .Where(s => s.CreatedBy == by)
                .Select(s => s.OrderNumber)
                .FirstOrDefault();

            if (lastOrderSearches != orderNumber)
            {
                db.OrderSearches.Add(new OrderSearch()
                {
                    OrderNumber = orderNumber,
                    CreateDate = when,
                    CreatedBy = by
                });
                db.Commit();
            }
        }
    }
}