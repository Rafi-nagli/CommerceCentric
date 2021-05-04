using System;
using Amazon.Common.Helpers;
using Amazon.Core.Helpers;
using Amazon.Core.Models;
using Amazon.DTO;
using Amazon.Model.Implementation.Markets;
using UrlHelper = Amazon.Web.Models.UrlHelper;

namespace Amazon.Web.ViewModels
{
    public class OrderItemViewModel
    {
        public long Id { get; set; }
        public long EntityId { get; set; }

        public long? ListingEntityId { get; set; }

        public int Market { get; set; }
        public string MarketplaceId { get; set; }
        public string FulfillmentChannel { get; set; }

        public string OrderId { get; set; }
        public string CustomerOrderId { get; set; }
        public string MarketOrderId { get; set; }
        public long? BatchId { get; set; }
        public string OrderStatus { get; set; }
        public string Quantity { get; set; }
        public int QuantityShipped { get; set; }
        public int QuantityOrdered { get; set; }

        public int? ShippingGroupId { get; set; }
        public int ShippingMethodId { get; set; }
        public string ShippingMethodName { get; set; }
        public string ShippingPackageName { get; set; }
        public string InitialServiceType { get; set; }
        public int? UpgradeLevel { get; set; }
        public int ShippingCalculationStatus { get; set; }

        public string FormattedShippingMethodName { get; set; }

        public string BuyerName { get; set; }
        public string PersonName { get; set; }
        public bool HasPhoneNumber { get; set; }
        public string ShippingCountry { get; set; }
        public string ShippingState { get; set; }
        public string ShippingCity { get; set; }

        public DateTime? OrderDate { get; set; }
        public DateTime? ExpectedShipDate { get; set; }

        public DateTime? AlignedExpectedShipDate
        {
            get
            {
                return ShippingUtils.AlignMarketDateByEstDayEnd(ExpectedShipDate, (MarketType)Market);
            }    
        }

        public double? Weight { get; set; }

        public string WeightString
        {
            get
            {
                if (!Weight.HasValue || Weight == 0)
                    return "";

                return WeightHelper.FormatWeight(Weight.Value);
            }
        }


        public string PictureUrl { get; set; }

        public string StyleID { get; set; }
        public string StyleSize { get; set; }
        public string Size { get; set; }
        public string Color { get; set; }
        public long? StyleItemId { get; set; }


        public string SourceStyleString { get; set; }
        public string SourceStyleSize { get; set; }
        public string SourceStyleColor { get; set; }


        public string ASIN { get; set; }
        public string ParentASIN { get; set; }
        public string Title { get; set; }
        public DateTime? ListingCreateDate { get; set; }

        public string ItemOrderId { get; set; }
        public string RecordNumber { get; set; }

        public decimal? FBAFee { get; set; }
        public int? FBAAvailableQuantity { get; set; }
        public string SimilarNonFBASKU { get; set; }
        public decimal? SimilarNonFBAPrice { get; set; }


        public decimal ItemPrice { get; set; }
        public decimal? ItemPriceInUSD { get; set; }

        public decimal? RefundItemPrice { get; set; }
        public decimal? ItemDiscount { get; set; }
        public decimal? ItemTax { get; set; }
        public decimal ShippingPrice { get; set; }

        public decimal? ShippingDiscount { get; set; }

        public decimal? RefundShippingPrice { get; set; }

        public decimal ShippingPriceInUSD
        {
            get { return PriceHelper.RougeConvertToUSD(Currency, ShippingPrice); }
        }

        public bool HasShippingPrice
        {
            get { return ShippingPrice > 0; }
        }

        public bool HasItemRefund
        {
            get { return RefundItemPrice > 0; }
        }

        public bool HasShippingRefund
        {
            get { return RefundShippingPrice > 0; }
        }

        public decimal? ExcessiveShipmentThreshold { get; set; }


        public string StyleUrl
        {
            get { return UrlHelper.GetStyleUrl(StyleID); }
        }

        public string StylePopoverInfoUrl
        {
            get { return UrlHelper.GetStylePopoverInfoUrl(StyleID); }
        }

        public string StyleWithListingPopoverInfoUrl
        {
            get { return UrlHelper.GetStylePopoverInfoUrl(StyleID, ListingEntityId); }
        }

        public string SourceStyleUrl
        {
            get { return UrlHelper.GetStyleUrl(SourceStyleString); }
        }

        public string SourceStylePopoverInfoUrl
        {
            get { return UrlHelper.GetStylePopoverInfoUrl(SourceStyleString); }
        }

        public string Thumbnail
        {
            get
            {
                return UrlHelper.GetThumbnailUrl(PictureUrl, 0, 75, false, ImageHelper.NO_IMAGE_URL, convertInDomainUrlToThumbnail:true);
            }
        }

        public long LocationIndex { get; set; }        

        public bool OnHold { get; set; }

        public bool IsDisabled
        {
            get
            {
                return false; //IsPrime; 
            }
        }
        
        public string ProductUrl
        {
            get
            {
                return UrlHelper.GetProductUrl(ParentASIN, (MarketType)Market, MarketplaceId);
            }
        }

        public string ProductStyleUrl
        {
            get
            {
                return UrlHelper.GetProductByStyleUrl(StyleID, (MarketType)Market, MarketplaceId);
            }
        }

        public string SourceMarketId { get; set; }

        public string MarketUrl
        {
            get
            {
                return UrlHelper.GetMarketUrl(ASIN, SourceMarketId, (MarketType) Market, MarketplaceId);
            }
        }

        public string SellerUrl
        {
            get { return UrlHelper.GetSellerCentralOrderUrl((MarketType) Market, MarketplaceId, OrderId); }
        }

        public string NewEmailUrl
        {
            get { return UrlHelper.GetNewEmailUrl(OrderId); }
        }

        public string OrderEmailsUrl
        {
            get { return UrlHelper.GetOrderEmailsUrl(OrderId); }
        }

        public string Currency
        {
            get { return PriceHelper.GetCurrencySymbol((MarketType)Market, MarketplaceId); }
        }
        

        public bool IsOrder { get { return false; } }

        public int NumberByLocation { get; set; }
        public bool HasAddressLengthIssue { get; set; }

        public bool UIChecked { get; set; }
        public bool UIDisabled { get; set; }
        
        public string BayNumber { get; set; }

        public string MarketName
        {
            get
            {
                return MarketHelper.GetMarketName(Market, MarketplaceId);
            }
        }

        public OrderItemViewModel()
        {

        }
        
        /// <summary>
        /// For Named List
        /// </summary>
        /// <param name="item"></param>
        /// <param name="market"></param>
        /// <param name="isOrderOnHold"></param>
        /// <param name="isOrderPartial"></param>
        public OrderItemViewModel(ListingOrderDTO item, bool isOrderOnHold, bool isOrderPartial)
        {
            Market = item.Market;
            MarketplaceId = item.MarketplaceId;

            ListingEntityId = item.Id;

            ItemOrderId = item.ItemOrderId;
            RecordNumber = item.RecordNumber;
            ParentASIN = item.ParentASIN;
            ASIN = item.ASIN;            

            ListingCreateDate = item.ListingCreateDate;

            SourceMarketId = item.SourceMarketId;

            OnHold = isOrderOnHold;
            
            StyleID = item.StyleID;
            StyleSize = item.StyleSize;
            Size = item.Size;
            Color = item.Color;
            StyleItemId = item.StyleItemId;


            SourceStyleString = item.SourceStyleString;
            SourceStyleSize = item.SourceStyleSize;
            SourceStyleColor = item.SourceStyleColor;

            Title = item.Title;
            Quantity = isOrderPartial
                ? item.QuantityShipped.ToString("G") + "/" + item.QuantityOrdered.ToString("G")
                : item.QuantityOrdered.ToString("G");
            QuantityOrdered = item.QuantityOrdered;
            QuantityShipped = item.QuantityShipped;

            if (item.ItemPaid.HasValue && item.ItemPaid > 0)
                ItemPrice = item.ItemPaid.Value - (item.ItemTax ?? 0);
            else if (Market == (int)MarketType.eBay)
                ItemPrice = item.ItemPrice * item.QuantityOrdered;
            else
                ItemPrice = item.ItemPrice;
            RefundItemPrice = item.RefundItemPrice;
            ItemDiscount = item.PromotionDiscount;
            ItemTax = item.ItemTax;
            ShippingPrice = item.ShippingPrice;
            RefundShippingPrice = item.RefundShippingPrice;
            ShippingDiscount = item.ShippingDiscount;
            ItemPriceInUSD = item.ItemPriceInUSD;

            ExcessiveShipmentThreshold = StringHelper.TryGetDecimal(item.ExcessiveShipmentThreshold);

            FBAFee = item.FBAPerOrderFulfillmentFee + item.FBAPerUnitFulfillmentFee + item.FBAWeightBasedFee;
            FBAAvailableQuantity = item.AvailableQuantity;
            SimilarNonFBASKU = item.SimilarNonFBASKU;
            SimilarNonFBAPrice = item.SimilarNonFBAPrice;

            var image = item.ItemPicture;
            if (String.IsNullOrEmpty(image))
                image = item.StyleImage;
            if (item.ReplaceType == (int) ItemReplaceTypes.Combined)
                image = item.StyleImage;
            if (item.UseStyleImage)
                image = item.StyleImage;

            PictureUrl = ImageHelper.GetFirstOrDefaultPicture(image);
            
            Weight = item.Weight;

            BayNumber = StringHelper.Substring(item.SortIsle.ToString(), 1);
            LocationIndex = LocationHelper.GetLocationIndex(item.SortIsle, item.SortSection, item.SortShelf);
        }
    }
}