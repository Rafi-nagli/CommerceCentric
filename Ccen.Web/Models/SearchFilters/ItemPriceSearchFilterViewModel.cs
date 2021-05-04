using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Amazon.Common.Helpers;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models;


namespace Amazon.Web.Models
{
    public class ItemPriceSearchFilterViewModel
    {
        public int? MainLicense { get; set; }
        public int? SubLicense { get; set; }

        public int? Gender { get; set; }
        public int? NoneSoldPeriod { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        public MarketType Market { get; set; }
        public string MarketplaceId { get; set; }

        public BuyBoxWinModes BuyBoxWinMode { get; set; }
        public ListingsModeType ListingMode { get; set; }
        public ProductAvailability Availability { get; set; }
        public string StyleString { get; set; }


        public ItemPriceSearchFilterViewModel()
        {
            
        }

        public override string ToString()
        {
            return "MainLicense=" + MainLicense
                   + ", SubLicense=" + SubLicense
                   + ", Gender=" + Gender
                   + ", NoneSoldPeriod=" + NoneSoldPeriod
                   + ", MinPrice=" + MinPrice
                   + ", MaxPrice=" + MaxPrice
                   + ", MarketplaceId=" + MarketplaceId
                   + ", Market=" + Market
                   + ", BuyBoxWinMode=" + BuyBoxWinMode
                   + ", ListingMode=" + ListingMode
                   + ", ProductAvailability=" + Availability
                   + ", StyleString=" + StyleString;
        }

        public static SelectList AvailabilityList
        {
            get
            {
                return new SelectList(new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>(ProductAvailabilityHelper.GetName(ProductAvailability.All), ProductAvailability.All.ToString()),
                    new KeyValuePair<string, string>(ProductAvailabilityHelper.GetName(ProductAvailability.InStock), ProductAvailability.InStock.ToString())
                }, "Value", "Key", ProductAvailability.InStock.ToString());
            }
        }

        public static SelectList FBAListingsModeList
        {
            get
            {
                return new SelectList(new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>(ListingModeHelper.GetName(ListingsModeType.All), ((int)ListingsModeType.All).ToString()),
                    new KeyValuePair<string, string>(ListingModeHelper.GetName(ListingsModeType.OnlyFBA), ((int)ListingsModeType.OnlyFBA).ToString()),
                    new KeyValuePair<string, string>(ListingModeHelper.GetName(ListingsModeType.WithoutFBA), ((int)ListingsModeType.WithoutFBA).ToString()),
                }, "Value", "Key", ((int)ListingsModeType.All).ToString());
            }
        }

        public static SelectList SellingHistoryList
        {
            get
            {
                return new SelectList(new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("All", ""),
                    new KeyValuePair<string, string>("None sold in a Week", "7"),
                    new KeyValuePair<string, string>("None sold in a Month", "30"),
                    new KeyValuePair<string, string>("None sold in 3 Month", "90"),
                }, "Value", "Key");
            }
        }
    }
}