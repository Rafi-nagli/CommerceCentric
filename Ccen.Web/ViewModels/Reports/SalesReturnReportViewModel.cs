using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models;
using Amazon.Web.General.Models;
using Amazon.Web.Models;
using Ccen.Web.ViewModels.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Amazon.Web.ViewModels.Reports
{
    public class SalesReturnReportViewModel
    {
        public string StyleString { get; set; }
        public long StyleId { get; set; }
        public string Image { get; set; }
        public int? ReturnsUnits { get; set; }
        public int SoldUnits { get; set; }
        public int? RemainingUnits { get; set; }


        public string StyleUrl
        {
            get
            {
                return UrlHelper.GetStyleUrl(StyleString);
            }
        }

        public string Thumbnail
        {
            get
            {
                return UrlManager.UrlService.GetThumbnailUrl(Image,
                    75,
                    75,
                    false,
                    ImageHelper.NO_IMAGE_URL,
                    false,
                    false,
                    convertInDomainUrlToThumbnail: true);
            }
        }


        public GridResponse<SalesReturnReportViewModel> GetAll(IUnitOfWork db, SalesExtReportFiltersViewModel filters)
        {
            var query = from o in db.Orders.GetAll()
                        join oi in db.OrderItems.GetAll() on o.Id equals oi.OrderId
                        join l in db.Listings.GetViewListings() on oi.ListingId equals l.Id
                        join stCache in db.StyleCaches.GetAll() on l.StyleId equals stCache.Id
                        where o.OrderStatus == OrderStatusEnumEx.Shipped
                        select new { o, oi, l, stCache };

            if (filters.DateFrom.HasValue)
            {
                query = query.Where(o => o.o.OrderDate >= filters.DateFrom.Value);
            }
            if (filters.DateTo.HasValue)
            {
                query = query.Where(o => o.o.OrderDate >= filters.DateTo.Value);
            }
            if (filters.Market.HasValue)
            {
                query = query.Where(o => o.o.Market == filters.Market.Value);
            }
            if (!String.IsNullOrEmpty(filters.MarketplaceId))
            {
                query = query.Where(o => o.o.MarketplaceId == filters.MarketplaceId);
            }
            if (!String.IsNullOrEmpty(filters.Keywords))
            {
                query = query.Where(o => o.oi.StyleString == filters.Keywords
                    || o.l.Barcode == filters.Keywords);
            }

            if (filters.ItemStyles != null && filters.ItemStyles.Any())
            {
                var itemStyleQuery = db.StyleFeatureValues.GetAll()
                    .Where(f => filters.ItemStyles.Contains(f.FeatureValueId))
                    .Select(f => f.StyleId)
                    .ToList();

                query = from st in query
                        join i in itemStyleQuery on st.stCache.Id equals i
                        select st;
            }

            if (filters.Genders != null && filters.Genders.Any())
            {
                var genderIds = filters.Genders.Select(g => g.ToString()).ToList();
                query = query.Where(st => genderIds.Contains(st.stCache.Gender));
            }

            if (filters.MainLicense.HasValue)
            {
                if (filters.MainLicense == 0)
                    query = query.Where(st => String.IsNullOrEmpty(st.stCache.MainLicense));
                else
                    query = query.Where(st => st.stCache.MainLicense == filters.MainLicense.Value.ToString());
            }

            if (filters.SubLicense.HasValue)
            {
                query = query.Where(st => st.stCache.SubLicense == filters.SubLicense.Value.ToString());
            }


            var baseItemsQuery = query.GroupBy(o => o.oi.StyleId).Select(o =>
                new
                {
                    StyleId = o.Key,
                    ReturnsUnits = o.Sum(i => i.oi.QuantityRefunded)
                });

            var returnUnits = baseItemsQuery.Where(i => i.ReturnsUnits > 0).ToList();

            if (filters.DateFrom.HasValue)
                filters.DateFrom = filters.DateFrom.Value.AddMonths(-2);

            var allItems = new SalesExtReportViewModel().GetAll(db,
                filters).Items;

            var items = new List<SalesReturnReportViewModel>();
            foreach (var returnUnit in returnUnits)
            {
                var item = allItems.FirstOrDefault(i => i.StyleId == returnUnit.StyleId);
                items.Add(new SalesReturnReportViewModel()
                {
                    StyleId = item.StyleId,
                    StyleString = item.StyleString,
                    Image = item.Image,
                    RemainingUnits = item.RemainingUnits,
                    ReturnsUnits = returnUnit.ReturnsUnits,
                    SoldUnits = item.SoldUnits,
                });
            }

            switch (filters.SortField)
            {
                case "StyleString":
                    if (filters.SortMode == 0)
                        items = items.OrderBy(o => o.StyleString).ToList();
                    else
                        items = items.OrderByDescending(o => o.StyleString).ToList();
                    break;
                //case "BrandName":
                //    if (filter.SortMode == 0)
                //        styleWithCacheQuery = styleWithCacheQuery.OrderBy(s => s.stCache.Brand);
                //    else
                //        styleWithCacheQuery = styleWithCacheQuery.OrderByDescending(s => s.stCache.Brand);
                //    break;
                case "SoldUnits":
                    if (filters.SortMode == 0)
                        items = items.OrderBy(s => s.SoldUnits).ToList();
                    else
                        items = items.OrderByDescending(s => s.SoldUnits).ToList();
                    break;
                case "RemainingUnits":
                    if (filters.SortMode == 0)
                        items = items.OrderBy(s => s.SoldUnits).ToList();
                    else
                        items = items.OrderByDescending(s => s.SoldUnits).ToList();
                    break;
                case "ReturnsUnits":
                    if (filters.SortMode == 0)
                        items = items.OrderBy(s => s.ReturnsUnits).ToList();
                    else
                        items = items.OrderByDescending(s => s.ReturnsUnits).ToList();
                    break;
                default:
                    items = items.OrderBy(s => s.StyleString).ToList();
                    break;
            }

            return new GridResponse<SalesReturnReportViewModel>(items, items.Count);
        }

        public static IList<ReturnedOrderViewModel> GetAllOrders(IUnitOfWork db, long styleId)
        {
            var query = from o in db.Orders.GetAll()
                        join oi in db.OrderItems.GetAll() on o.Id equals oi.OrderId
                        join l in db.Listings.GetViewListings() on oi.ListingId equals l.Id
                        join stCache in db.StyleCaches.GetAll() on l.StyleId equals stCache.Id                        
                        join r in db.ReturnRequests.GetFiltered(x => x.StyleId == styleId)
                        on o.CustomerOrderId equals r.OrderNumber
                        into joinedRequests
                        from jr in joinedRequests.DefaultIfEmpty()
                        join stIt in db.StyleItems.GetAll() 
                        on jr.StyleItemId equals stIt.Id
                        into joinedItems 
                        from jsi in joinedItems.DefaultIfEmpty()
                       
                        
                        where o.OrderStatus == OrderStatusEnumEx.Shipped && l.StyleId == styleId && oi.QuantityRefunded > 0

                        select new { o, oi, l, stCache, jr, jsi };

            var baseItemsQuery = query.Select(o =>
                new ReturnedOrderViewModel
                {
                    Id = o.o.Id,
                    CustomerOrderId = o.o.CustomerOrderId,
                    OrderDate = o.o.OrderDate,
                    Reason = o.jr == null ? "" : o.jr.Reason,
                    StyleItemId = o.jsi == null ? (long?)null : o.jsi.Id,
                    Size = o.jsi == null ? "" : o.jsi.Size,
                    Color = o.jsi == null ? "" : o.jsi.Color,
                }).OrderByDescending(x => x.OrderDate).Distinct();

            var res = baseItemsQuery.ToList();
            return res;
        }
    }
}