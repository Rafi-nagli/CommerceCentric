using System;
using System.Globalization;
using Amazon.Api;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Parser;
using Amazon.Core.Entities;
using Amazon.Core.Models;
using Amazon.DTO;

namespace Amazon.ReportParser.LineParser
{
    public class ListingLineParser : ILineParser
    {
        private const int ItemName = 0;
        private const int ItemDescription = 1;
        private const int ListingID = 2;
        private const int SellerSku = 3;
        private const int PriceIndex = 4;
        private const int QuantityIndex = 5;
        private const int OpenDate = 6;
        //private const int ImageURL = 7;
        private const int ProductIDType = 9;
        private const int ItemNote = 11;
        private const int BusinessPriceIndex = 27;
        private const int ASIN1 = 16;
        private const int WillShipInternationally = 19;
        private const int ShippingGroup = 39;
        
        private const int Length = 27;

        private ILogService _log;

        public ListingLineParser(ILogService log)
        {
            _log = log;
        }

        public IReportItemDTO Parse(string[] fields, string[] headers)
        {
            //string[] fields = line.Split('	');
            //if (fields.Length < Length)
            //{
            //    return null;
            //}
            var item = new ItemDTO();

            for (var i = 0; i < fields.Length; i++)
            {
                try
                {
                    var val = fields[i];
                    var header = headers[i];
                    switch (header)
                    {
                        case "item-name":
                            item.Name = val;
                            break;
                        case "item-description":
                            item.Description = val;
                            break;
                        case "listing-id":
                            item.ListingId = val;
                            break;
                        case "seller-sku":
                            item.SKU = val;
                            if (item.SKU.Contains("-FBA"))
                                item.IsFBA = true;
                            if (item.SKU.Contains("-FPB"))
                                item.IsPrime = true;
                            break;
                        case "price":
                            item.CurrentPrice = LineParserHelper.GetPrice(val) ?? 0;
                            break;
                        case "quantity":
                            item.RealQuantity = 0;
                            
                            if (!string.IsNullOrEmpty(val))
                                item.RealQuantity = LineParserHelper.GetAmount(val) ?? 0;
                            else
                                item.IsFBA = true;
                            break;
                        case "open-date":
                            item.OpenDate = LineParserHelper.GetDateVal(_log, val, OpenDate, "");
                            break;
                        case "image-url":
                            break;
                        case "item-is-marketplace":
                            break;
                        case "product-id-type":
                            item.ProductIdType = LineParserHelper.GetAmount(val) ?? 0;
                            item.PublishedStatus = (int)PublishedStatuses.Published;

                            //switch (item.ProductIdType)
                            //{
                            //    case 3:
                            //        item.PublishedStatus = (int) PublishedStatuses.Published;
                            //        break;
                            //    case 1:
                            //        item.PublishedStatus = (int) PublishedStatuses.PublishedInactive;
                            //        break;
                            //    default:
                            //        item.PublishedStatus = (int) PublishedStatuses.None;
                            //        break;
                            //}
                            break;
                        case "item-note":
                            item.Note = val;
                            break;
                        case "asin1":
                            item.ASIN = val;
                            item.SourceMarketId = val;
                            break;
                        case "will-ship-internationally":
                            item.IsInternational = LineParserHelper.GetBoolVal(val);
                            break;
                        case "Business Price":
                            item.BusinessPrice = LineParserHelper.TryGetPrice(val);
                            break;
                        case "merchant-shipping-group":
                            item.OnMarketTemplateName = val;
                            if (item.OnMarketTemplateName == AmazonTemplateHelper.PrimeTemplate)
                                item.IsPrime = true;
                            break;
                    }
                }
                catch (Exception ex)
                {
                    _log.Error(string.Format("Unable to parse field: {0}", fields[i]), ex);
                }
            }
            return item;
        }
    }
}
