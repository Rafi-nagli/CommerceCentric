using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Helpers;
using Amazon.Core.Models;
using Amazon.DAL;
using Amazon.DTO;
using Amazon.Model.Implementation.Markets;
using Amazon.Model.Implementation.Sorting;
using Amazon.Web.Models;
using log4net;

namespace Amazon.Web.ViewModels
{
    public class PackingSlipCollectionModel
    {
        public DateTime Date { get; set; }
        public string BatchName { get; set; }
        public long? BatchId { get; set; }

        public AddressDTO ReturnAddress { get; set; }
        public string ReteurnAddressString
        {
            get
            {
                // P.O. Box 398, Hallandale Beach, FL, 33008
                return AddressHelper.ToString(ReturnAddress, ", ");
            }
        }

        public IList<PackingSlipMarketplaceInfo> Marketplaces { get; set; }

        public PackingSlipMarketplaceInfo GetMarketInfoBy(MarketType market, string marketplaceId)
        {
            var marketInfo = Marketplaces.FirstOrDefault(m => m.Market == (int)market
                                                    && (String.IsNullOrEmpty(m.MarketplaceId) ||
                                                     m.MarketplaceId == marketplaceId));
            return marketInfo ?? PackingSlipMarketplaceInfo.Empty();
        }

        public IList<PackingSlipViewModel> PackingSlips { get; set; }
    }

    public class PackingSlipMarketplaceInfo
    {
        public int Market { get; set; }
        public string MarketplaceId { get; set; }

        public string MarketName
        {
            get { return MarketHelper.GetPackingSlipDisplayName(Market, MarketplaceId); }
        }

        public string MarketLogoUrl
        {
            get { return MarketHelper.GetMarketLogo(Market, MarketplaceId); }
        }

        public string StoreDisplayName { get; set; }
        public string StoreLogoUrl { get; set; }
        public string StoreUrl { get; set; }
        public string PackingSlipFooterTemplate { get; set; }

        public PackingSlipMarketplaceInfo()
        {
            
        }

        public PackingSlipMarketplaceInfo(MarketplaceDTO marketplace)
        {
            Market = marketplace.Market;
            MarketplaceId = marketplace.MarketplaceId;

            StoreDisplayName = marketplace.DisplayName;
            StoreLogoUrl = marketplace.StoreLogo;
            StoreUrl = marketplace.StoreUrl;

            PackingSlipFooterTemplate = marketplace.PackingSlipFooterTemplate;
        }

        public static PackingSlipMarketplaceInfo Empty()
        {
            return new PackingSlipMarketplaceInfo();
        }
    }

    public class PackingSlipViewModel
    {
        public class PackingSlipItemViewModel
        {
            public string ImageUrl { get; set; }
            public string Name { get; set; }
            public string Size { get; set; }
            public string Color { get; set; }
            public string SKU { get; set; }
            public string StyleID { get; set; }
            public int Quantity { get; set; }

            public string StyleSize { get; set; }
            public string StyleColor { get; set; }

            public string ASIN { get; set; }

            public string Isle { get; set; }
            public string Section { get; set; }
            public string Shelf { get; set; }

            public string OrderItemID { get; set; }
            public string ListingID { get; set; }

            public string FormattedSize
            {
                get
                {
                    return SizeHelper.UIFormatSize(Size);
                }
            }

            public string FormattedStyleSize
            {
                get
                {
                    return SizeHelper.UIFormatSize(StyleSize);
                }
            }

            public string Thumbnail
            {
                get
                {
                    return UrlHelper.GetThumbnailUrl(ImageUrl, 150, 0, false, ImageHelper.NO_IMAGE_URL, convertInDomainUrlToThumbnail:true);
                }
            }
        }

        public int Number { get; set; }
        public int BatchNumber { get; set; }

        public string AddressToFullName { get; set; }
        public string AddressToAddress1 { get; set; }
        public string AddressToAddress2 { get; set; }
        public string AddressToCity { get; set; }
        public string AddressToState { get; set; }
        public string AddressToZip { get; set; }
        public string AddressToCountry { get; set; }

        public string AddressToPhone { get; set; }

        public string OrderId { get; set; }
        public MarketType Market { get; set; }
        public string MarketplaceId { get; set; }


        public string FormattedOrderId
        {
            get { return OrderHelper.FormatOrderNumber(OrderId, Market); }
        }

        public string BuyerName { get; set; }
        public string PersonName { get; set; }
        public DateTime? OrderDate { get; set; }


        public string OrderDateFormatted
        {
            get
            {
                return OrderDate.HasValue
                    ? OrderDate.Value.ToString("MM/dd/yyyy HH:mm:ss")
                    : String.Empty;
            }
        }

        public string FormattedBuyerName
        {
            get
            {
                return String.IsNullOrEmpty(BuyerName) ? PersonName : BuyerName;
            }
        }

        public string Carrier { get; set; }

        //Shows actual shipping method, from Order Shipping Info
        public string ShippingService { get; set; }

        
        public IList<PackingSlipItemViewModel> Items { get; set; }


        public static IEnumerable<PackingSlipViewModel> GetList(IUnitOfWork db, 
            long[] orderIdList, 
            SortMode sortOrder,
            bool includeOnHold)
        {
            IList<PackingListDTO> orders = db.OrderShippingInfos.GetPackingSlipOrders(orderIdList, sortOrder, unmaskReferenceStyle:true).ToList();

            if (!includeOnHold)
                orders = orders.Where(o => !o.OnHold).ToList();

            var allHasNumbers = orders.All(o => o.NumberInBatch.HasValue);
            if (allHasNumbers)
                orders = orders.OrderBy(o => o.NumberInBatch).ToList();
            else
                orders = SortHelper.Sort(orders, sortOrder);

            var models = orders.Select((t, i) => new PackingSlipViewModel(t, allHasNumbers ? (t.NumberInBatch ?? (i + 1)) : i + 1));
            return models;
        }

        public PackingSlipViewModel(PackingListDTO order, int number)
        {
            Number = number;
            BatchNumber = order.BatchId.HasValue ? (int)order.BatchId.Value : 0;
            AddressToFullName = order.FinalPersonName;
            AddressToPhone = order.FinalShippingPhone;

            AddressToAddress1 = order.FinalShippingAddress1;
            AddressToAddress2 = order.FinalShippingAddress2;
            AddressToCity = order.FinalShippingCity;
            AddressToZip = ShippingUtils.CombineZip(order.FinalShippingZip, order.FinalShippingZipAddon);
            AddressToState = order.FinalShippingState;
            AddressToCountry = order.FinalShippingCountry;
            
            OrderId = order.OrderId;
            Market = (MarketType) order.Market;
            MarketplaceId = order.MarketplaceId;

            PersonName = order.PersonName;
            BuyerName = order.BuyerName;
            OrderDate = order.OrderDate;

            Carrier = order.Carrier;
            if (order.StampsShippingCost == 0 || !order.StampsShippingCost.HasValue)
                ShippingService = order.InitialServiceType;
            else
                ShippingService = order.ShippingMethodName;
            
            Items = new List<PackingSlipItemViewModel>();
            foreach (var item in order.Items)
            {
                var image = item.ItemPicture;
                if (String.IsNullOrEmpty(image))
                    image = item.StyleImage;

                //TEMP: always style image
                //NOTE: Disabled, back to listing images
                //image = item.StyleImage;

                if (!String.IsNullOrEmpty(item.StyleImage)
                    && item.SourceStyleString != item.StyleID)
                    //NOTE: happens when item has linked style OR item style was changed
                    image = item.StyleImage;

                if (item.UseStyleImage)
                    image = item.StyleImage;

                Items.Add(new PackingSlipItemViewModel
                {
                    ASIN = item.ASIN,

                    ImageUrl = ImageHelper.GetFirstOrDefaultPicture(image),
                    Quantity = item.QuantityOrdered,
                    ListingID = item.ListingId,
                    Name = item.Title,
                    OrderItemID = item.ItemOrderId,
                    Size = item.Size,
                    Color = item.Color,
                    SKU = item.SKU,
                    StyleID = item.StyleID,

                    StyleSize = item.StyleSize,
                    StyleColor = item.StyleColor,

                    Isle = item.DefaultLocation?.Isle,
                    Section = item.DefaultLocation?.Section,
                    Shelf = item.DefaultLocation?.Shelf
                });
            }
        }
    }
}