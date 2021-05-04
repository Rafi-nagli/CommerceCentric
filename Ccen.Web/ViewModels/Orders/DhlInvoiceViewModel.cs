using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Amazon.Common.ExcelExport;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.DTO.Shippings;
using Amazon.Web.Models;
using Amazon.Web.Models.SearchFilters;
using Amazon.Web.ViewModels.Messages;

namespace Amazon.Web.ViewModels.Orders
{
    public class DhlInvoiceViewModel
    {
        public int Id { get; set; }

        public string OrderNumber { get; set; }
        public DateTime? OrderDate { get; set; }
        public int Market { get; set; }
        public string MarketplaceId { get; set; }


        public double? WeightUsed { get; set; }

        public int Status { get; set; }

        public DateTime InvoiceDate { get; set; }
        public string InvoiceNumber { get; set; }

        public string Country { get; set; }
        public string RateCode { get; set; }
        public decimal? RatePrice { get; set; }
        public string Dimensions { get; set; }

        public string BillNumber { get; set; }
        public decimal Charged { get; set; }
        public decimal? Estimated { get; set; }

        public string StatusString
        {
            get { return DhlInvoiceHelper.ToString((DhlInvoiceStatusEnum)Status); }
        }

        public string FormattedAdjustedWeightUsed
        {
            get { return WeightUsed.HasValue ? WeightUsed.Value.ToString() : null; }
        }

        public string OrderUrl
        {
            get { return UrlHelper.GetOrderUrl(OrderNumber); }
        }

        public DhlInvoiceViewModel()
        {
            
        }

        public DhlInvoiceViewModel(DhlInvoiceDTO invoice)
        {
            Id = invoice.Id;
            OrderNumber = invoice.OrderNumber;

            Status = invoice.Status;

            InvoiceDate = invoice.InvoiceDate;
            InvoiceNumber = invoice.InvoiceNumber;

            Country = invoice.Country;
            RateCode = invoice.RateCode;
            Dimensions = invoice.Dimensions;

            BillNumber = invoice.BillNumber;
            Charged = invoice.ChargedSummary;
            Estimated = invoice.Estimated;
        }

        public static void SetStatus(IUnitOfWork db,
            int id,
            int newStatus)
        {
            var invoice = db.DhlInvoices.Get(id);
            if (invoice != null)
            {
                invoice.Status = newStatus;
                db.Commit();
            }
        }

        public static IQueryable<DhlInvoiceViewModel> GetAll(IUnitOfWork db,
            ITime time,
            DhlInvoiceFilterViewModel filter)
        {
            var weightQuery = from i in db.OrderItems.GetAllAsDto()
                join l in db.Listings.GetViewListings() on i.ListingId equals l.Id
                group new {i, l} by i.OrderId
                into byOrder
                select new
                {
                    OrderId = byOrder.Key,
                    Weight = byOrder.Sum(i => i.i.QuantityOrdered * (i.l.Weight ?? 0))
                };


            var query = from n in db.DhlInvoices.GetAllAsDto()
                        join o in db.Orders.GetAll() on n.OrderNumber equals o.AmazonIdentifier  
                        join rc in db.DhlRateCodes.GetAllAsDto() on o.ShippingCountry equals rc.CountryCode into withRC
                        from rc in withRC.DefaultIfEmpty()
                        join oi in weightQuery on o.Id equals oi.OrderId
                        select new DhlInvoiceViewModel
                        {
                            Id = n.Id,
                            OrderNumber = n.OrderNumber,
                            OrderDate = o.OrderDate,
                            Market = o.Market,
                            MarketplaceId = o.MarketplaceId,

                            Status = n.Status,

                            WeightUsed = oi.Weight,
                            Country = o.ShippingCountry,
                            RateCode = rc.RateCode,
                            Dimensions = n.Dimensions,

                            BillNumber = n.BillNumber,
                            Charged = n.ChargedSummary,
                            Estimated = n.Estimated,
                            
                            InvoiceDate = n.InvoiceDate,
                            InvoiceNumber = n.InvoiceNumber,
                        };

            if (!String.IsNullOrEmpty(filter.OrderNumber))
                query = query.Where(n => n.OrderNumber == filter.OrderNumber);

            if (filter.DateFrom.HasValue)
                query = query.Where(n => n.InvoiceDate >= filter.DateFrom.Value);

            if (filter.DateTo.HasValue)
                query = query.Where(n => n.InvoiceDate <= filter.DateTo.Value);

            if (filter.Status.HasValue)
                query = query.Where(n => n.Status == filter.Status.Value);

            var invoiceList = query.ToList();
            var rateList = db.DhlRateCodePrices.GetAllAsDto().ToList();
            foreach (var invoice in invoiceList)
            {
                var weight = invoice.WeightUsed.HasValue ? Math.Ceiling(invoice.WeightUsed.Value/(double)16) : 0;
                var rateCodes = rateList.Where(r => r.RateCode == invoice.RateCode && r.ProductCode == "P").OrderBy(r => r.Weight).ToList();
                var rate = rateCodes.FirstOrDefault(r => r.Weight == weight);
                if (rate != null)
                    invoice.RatePrice = rate.Price;
            }

            return invoiceList.AsQueryable();
        }


        public static MemoryStream BuildReport(IList<DhlInvoiceViewModel> invoices)
        {
            var b = new ExportColumnBuilder<DhlInvoiceViewModel>();
            var columns = new List<ExcelColumnInfo>()
            {
                b.Build(p => p.InvoiceNumber, "Invoice #", 15),
                b.Build(p => p.InvoiceDate, "Invoice Date", 15),
                b.Build(p => p.BillNumber, "Airbill", 15),
                b.Build(p => p.WeightUsed, "Total Weight ", 15),
                b.Build(p => p.Estimated, "Estimated", 15),
                b.Build(p => p.Charged, "Charged", 15),
                
                b.Build(p => p.OrderDate, "Order Date", 15),
                b.Build(p => p.OrderNumber, "Order #", 21),
            };

            return ExcelHelper.Export(invoices.OrderBy(i => i.InvoiceNumber).ToList(), columns);
        }

    }
}