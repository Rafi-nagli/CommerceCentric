using Amazon.Common.ExcelExport;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Entities.Caches;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Exports.Attributes;
using Amazon.DTO;
using Amazon.DTO.Inventory;
using Amazon.DTO.Orders;
using Amazon.Web.General.Models;
using Amazon.Web.Models;
using Amazon.Web.ViewModels.CustomReports;
using Amazon.Web.ViewModels.Reports;
using Ccen.Web.ViewModels.CustomReportView;
using Kendo.Mvc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Web;

namespace Ccen.Web.ViewModels.CustomReports.Entities
{
    public class CustomReportIncomeDisparityViewModel : CustomReportBaseDataViewModel
    {
        [ExcelSerializable("StyleId", Order = 0)]
        public string StyleString { get; set; }
        
        [DisplayInGrid(DisplayTypeEnum.Link, 2)]
        [CustomReportFieldAttribute(CustomReportFields.StyleID)]
        public LinkInfo StyleLink => new LinkInfo() { HRef = UrlHelper.GetStyleUrl(StyleString), Text = StyleString };
        
        [CustomReportField(CustomReportFields.Style_Id)]        
        public long StyleId { get; set; }
        public string Image { get; set; }


        [DisplayInGrid("Order Units", DisplayTypeEnum.General, 80, 4)]
        [ExcelSerializable("OrderUnits", Order = 1)]
        public int OrderUnits { get; set; }

        [DisplayInGrid("Min Price", DisplayTypeEnum.General, 80, 4)]
        [ExcelSerializable("MinPrice", Order = 2)]
        public decimal? MinPrice { get; set; }
        [DisplayInGrid("Max Price", DisplayTypeEnum.General, 80, 5)]
        [ExcelSerializable("MaxPrice", Order = 3)]
        public decimal? MaxPrice { get; set; }

        [CustomFilter(Id = 1, Header = "Date From", Operation = Amazon.DTO.CustomReports.FilterOperation.Greater, ValueString = "-14")]
        [CustomFilter(Id = 2, Header = "Date To", Operation = Amazon.DTO.CustomReports.FilterOperation.Less, ValueString = "0")]
        [CustomReportField(CustomReportFields.OrderDate)]
        public DateTime? OrderDate { get; set; }

        public DTOOrder Orders { get; set; }
        public StyleCache StyleCaches { get; set; }
        public OrderItemDTO OrderItems { get; set; }
        public StyleEntireDto Styles { get; set; }        
        
        public string NavigateUrl
        {
            get
            {
                return UrlHelper.GetStyleUrl(StyleString);
            }
        }

        [DisplayInGrid("", DisplayTypeEnum.LinkButton, 100, 100)]        
        public LinkButtonInfo StyleLinkButton => new LinkButtonInfo() { HRef = UrlHelper.GetStyleUrl(StyleString), Icon = "new-window", Text = "View" };

        [DisplayInGrid( DisplayTypeEnum.Image, 1, 100)]
        [CustomReportField(DataType = "string", Title = "")]
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

        public string OpenStyle
        {
            get
            {
                return UrlHelper.GetStyleUrl(StyleString);
            }
        }

        public override string SortField => "OrderDate";
        public override ListSortDirection SortOrder => ListSortDirection.Descending;        

        public override IQueryable GetAll(IUnitOfWork db, List<CustomReportFilterViewModel> filters)        
        {            
            var query = from o in db.Orders.GetAllAsDto()
                        join n in db.OrderNotifies.GetAllAsDto().Select(x=>x.OrderId).Distinct()
                        on o.Id equals n
                        join oi in db.OrderItems.GetAllAsDto() on o.Id equals oi.OrderId
                        join l in db.Listings.GetViewListings() on oi.ListingId equals l.Id                        
                        join stCache in db.StyleCaches.GetAll() on l.StyleId equals stCache.Id
                        where o.OrderStatus == OrderStatusEnumEx.Shipped
                        select new
                        { 
                            Orders = o,
                            OrderItems = oi,                            
                            StyleCaches = stCache,                            
                        };
            
            var whereString = CustomReportFilterViewModel.GetDynamicWhereClause(filters);
            var arrVals = filters.Select(x => x.Value).ToArray();
            if (!String.IsNullOrEmpty(whereString))
            {
                query = query.Where(whereString, arrVals);
            }
           
            var baseItemsQuery = query.GroupBy(o => new { o.OrderItems.StyleId, o.OrderItems.StyleString }).Select(o =>
                new CustomReportIncomeDisparityViewModel
                {
                    StyleId = o.Key.StyleId ?? 0,
                    StyleString = o.Key.StyleString,
                    OrderUnits = o.Sum(i => i.OrderItems.QuantityOrdered),
                    MinPrice = o.Min(i => i.OrderItems.ItemPriceInUSD.Value),
                    MaxPrice = o.Max(i => i.OrderItems.ItemPriceInUSD.Value),
                    OrderDate = o.Max(i => i.Orders.OrderDate)
                });

            baseItemsQuery = baseItemsQuery.OrderByDescending(x => x.OrderDate);            
            return baseItemsQuery;
        }

        public override IEnumerable GetTop1000(IUnitOfWork db, List<CustomReportFilterViewModel> filters)
        {
            var res = GetAll(db, filters) as IQueryable<CustomReportIncomeDisparityViewModel>;
            return res.Take(1000).ToList();
        }        
    }
}
