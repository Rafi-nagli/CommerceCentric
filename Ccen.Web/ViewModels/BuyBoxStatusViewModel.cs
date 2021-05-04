using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Models;
using Amazon.DTO;
using UrlHelper = Amazon.Web.Models.UrlHelper;

namespace Amazon.Web.ViewModels
{
    public class BuyBoxStatusViewModel
    {
        public long EntityId { get; set; }

        public string ASIN { get; set; }
        public MarketType Market { get; set; }
        public string MarketplaceId { get; set; }

        public string SKU { get; set; }
        public string Images { get; set; }

        public string WinnerMerchantName { get; set; }
        public decimal? WinnerPrice { get; set; }
        public decimal? WinnerSalePrice { get; set; }
        public decimal? WinnerAmountSaved { get; set; }
        public BuyBoxStatusCode Status { get; set; }
        public DateTime CheckedDate { get; set; }
        public DateTime? LostWinnerDate { get; set; }
        public bool IsIgnored { get; set; }


        public decimal Price { get; set; }
        public int? Quantity { get; set; }
        public string ParentASIN { get; set; }
        public string Size { get; set; }

        public string StyleString { get; set; }
        public long? StyleId { get; set; }


        public string Thumbnail
        {
            get
            {
                if (String.IsNullOrEmpty(Images))
                    return String.Empty;
                var parts = Images.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                var image = parts[0];

                return UrlHelper.GetThumbnailUrl(image, 0, 75, false, ImageHelper.NO_IMAGE_URL, convertInDomainUrlToThumbnail:true);
            }
        }

        public string ProductUrl
        {
            get { return UrlHelper.GetProductUrl(ParentASIN, Market, MarketplaceId); }
        }

        public string MarketUrl
        {
            get { return UrlHelper.GetMarketUrl(ParentASIN, null, Market, MarketplaceId); }
        }

        public string StyleUrl
        {
            get { return !string.IsNullOrEmpty(StyleString) ? "/Inventory/Styles?styleId=" + HttpUtility.UrlEncode(StyleString) : string.Empty; }
        }

        public bool HasImage
        {
            get { return !String.IsNullOrEmpty(Thumbnail); }
        }

        public string FormattedLostWinnerDate
        {
            get
            {
                return Status == BuyBoxStatusCode.NotWin && LostWinnerDate.HasValue ? DateHelper.ConvertUtcToApp(LostWinnerDate.Value).ToString("MM-dd-yyyy HH:mm:ss") : "";
            }
        }

        public decimal? MinPrice
        {
            get
            {
                if (WinnerSalePrice.HasValue)
                    return WinnerSalePrice;
                return WinnerPrice;
            }
        }

        public string FormattedCheckedDate
        {
            get { return DateHelper.ConvertUtcToApp(CheckedDate).ToString("MM-dd-yyyy HH:mm:ss"); }
        }

        public string StatusFormatted
        {
            get
            {
                switch (Status)
                {
                    case BuyBoxStatusCode.Win:
                        return "Win";
                        break;
                    case BuyBoxStatusCode.NotWin:
                        return "Not win";
                        break;
                    case BuyBoxStatusCode.Undefined:
                        return "Undefined";
                        break;
                }
                return "Undefined";
            }
        }

        public string LostWinnerDateFormatted
        {
            get
            {
                if (LostWinnerDate.HasValue)
                    return DateHelper.ConvertToReadableString(DateTime.UtcNow - LostWinnerDate);
                return "-";
            }
        }

        public BuyBoxStatusViewModel()
        {
            
        }

        public static IQueryable<BuyBoxStatusViewModel> GetAll(IUnitOfWork db, 
            int market,
            string marketplaceId,
            FilterPeriod period, 
            bool inStock, 
            bool includeIgnored)
        {
            var items = db.BuyBoxStatus.GetAllWithItems()
                .Where(bb => !String.IsNullOrEmpty(bb.ParentASIN))
                .Select(bb => new BuyBoxStatusViewModel()
            {
                EntityId = bb.Id,

                ASIN = bb.ASIN,
                Market = (MarketType)bb.Market,
                MarketplaceId = bb.MarketplaceId,

                Status = bb.Status,
                CheckedDate = bb.CheckedDate,
                LostWinnerDate = bb.LostWinnerDate, 

                WinnerMerchantName = bb.WinnerMerchantName,
                WinnerPrice = bb.WinnerPrice,
                WinnerSalePrice = bb.WinnerSalePrice,
                WinnerAmountSaved = bb.WinnerAmountSaved,
                IsIgnored = bb.IsIgnored,
                
                Price = bb.Price,
                Quantity = bb.Quantity < 0 ? 0 : bb.Quantity,
                ParentASIN = bb.ParentASIN,
                Size = bb.Size,
                Images = bb.Images,
                SKU = bb.SKU,
                StyleId = bb.StyleId,
                StyleString = bb.StyleString
            });

            DateTime? fromDate = null;
            switch (period)
            {
                case FilterPeriod.NotWinAll:
                    items = items.Where(i => i.Status == BuyBoxStatusCode.NotWin);
                    break;
                case FilterPeriod.WinLostLastMonth:
                    fromDate = DateTime.UtcNow.AddDays(-30);
                    break;
                case FilterPeriod.WinLostLastWeek:
                    fromDate = DateTime.UtcNow.AddDays(-7);
                    break;
                case FilterPeriod.WinLostLastDay:
                    fromDate = DateTime.UtcNow.AddDays(-1);
                    break;
            }

            if (fromDate.HasValue)
                items = items.Where(i => i.LostWinnerDate >= fromDate.Value);

            if (!includeIgnored)
                items = items.Where(i => !i.IsIgnored);

            if (inStock)
                items = items.Where(i => i.Quantity > 0);

            if (market != 0)
                items = items.Where(i => (int)i.Market == market);

            if (!String.IsNullOrEmpty(marketplaceId))
                items = items.Where(i => i.MarketplaceId == marketplaceId);

            return items;
        }

        #region Search Filters

        public static SelectList PeriodList
        {
            get
            {
                return new SelectList(new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("All", ((int)FilterPeriod.All).ToString()),
                    new KeyValuePair<string, string>("Not winning", ((int)FilterPeriod.NotWinAll).ToString()),
                    new KeyValuePair<string, string>("Lost in last Month", ((int)FilterPeriod.WinLostLastMonth).ToString()),
                    new KeyValuePair<string, string>("Lost in last Week", ((int)FilterPeriod.WinLostLastWeek).ToString()),
                    new KeyValuePair<string, string>("Lost in last Day", ((int)FilterPeriod.WinLostLastDay).ToString()),
                }, "Value", "Key", ((int)FilterPeriod.NotWinAll).ToString());
            }
        }


        public enum FilterPeriod
        {
            All = 0,
            NotWinAll = 1,
            WinLostLastMonth = 2,
            WinLostLastWeek = 3,
            WinLostLastDay = 4
        }
        #endregion
    }
}