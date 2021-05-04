using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models;
using Amazon.DTO.Listings;
using Amazon.Model.Implementation.Markets;
using DocumentFormat.OpenXml.Wordprocessing;


namespace Amazon.Web.Models
{
    public class ProductSearchFilterViewModel
    {
        public int? MainLicense { get; set; }
        public int? SubLicense { get; set; }

        public string Keywords { get; set; }
        public long? StyleId { get; set; }
        public string StyleName { get; set; }

        public long? DropShipperId { get; set; }

        public ProductAvailability Availability { get; set; }
        public ListingsModeType? ListingsMode { get; set; }

        public List<int> Genders { get; set; }
        public string Brand { get; set; }
        public int? NoneSoldPeriod { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        public int? PublishedStatus { get; set; }

        public MarketType Market { get; set; }
        public string MarketplaceId { get; set; }

        public int StartIndex { get; set; }
        public int LimitCount { get; set; }


        public ProductSearchFilterViewModel()
        {
            
        }

        public ItemSearchFiltersDTO GetDto()
        {
            return new ItemSearchFiltersDTO()
            {
                Keywords = Keywords,
                StyleId = StyleId,
                StyleName = StyleName,
                DropShipperId = DropShipperId,
                Market = (int)Market,
                MarketplaceId = MarketplaceId,
                MarketCode = MarketHelper.GetShortName((int)Market, MarketplaceId),

                MinPrice = MinPrice,
                MaxPrice = MaxPrice,
                PublishedStatus = PublishedStatus,

                Brand = Brand,
                Genders = Genders,

                Availability = (int)Availability,

                StartIndex = StartIndex,
                LimitCount = LimitCount,
            };
        }

        public override string ToString()
        {
            return "MainLicense=" + MainLicense
                               + ", SubLicense=" + SubLicense
                               + ", Gender=" + Genders
                               + ", Brand=" + Brand
                               + ", NoneSoldPeriod=" + NoneSoldPeriod
                               + ", MinPrice=" + MinPrice
                               + ", MaxPrice=" + MaxPrice
                               + ", MarketplaceId=" + MarketplaceId
                               + ", Market=" + Market;
        }

        public static SelectList AvailabilityList
        {
            get
            {
                return new SelectList(new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>(ProductAvailabilityHelper.GetName(ProductAvailability.All), ((int)ProductAvailability.All).ToString()),
                    new KeyValuePair<string, string>(ProductAvailabilityHelper.GetName(ProductAvailability.InStock), ((int)ProductAvailability.InStock).ToString())
                }, "Value", "Key", ((int)ProductAvailability.InStock).ToString());
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