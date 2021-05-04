using System;
using System.Collections.Generic;

using Amazon.Core.Models;

namespace Amazon.Web.Models
{
    public class StyleSearchFilterViewModel
    {
        public int StartIndex { get; set; }
        public int LimitCount { get; set; }

        public string SortField { get; set; }
        public int SortMode { get; set; }

        public string StyleString { get; set; }
        public long? StyleId { get; set; }
        public string Barcode { get; set; }

        public string Keywords { get; set; }
        public int? Gender { get; set; }
        public List<int> Genders { get; set; }

        public long? DropShipperId { get; set; }

        public bool OnlyInStock { get; set; }
        public bool IncludeKiosk { get; set; }
        public bool OnlyOnHold { get; set; }
        public List<int> ItemStyles { get; set; }
        public List<int> Sleeves { get; set; }

        public int? HolidayId { get; set; }
        public int? MainLicense { get; set; }
        public int? SubLicense { get; set; }
        public string BrandName { get; set; }
        
        public bool HasInitialQty { get; set; }
        public int? MinQty { get; set; }

        public string OnlineStatus { get; set; }
        public int? PictureStatus { get; set; }
        public int? FillingStatus { get; set; }
        public int? NoneSoldPeriod { get; set; }
        public string ExcludeMarketplaceId { get; set; }
        public string IncludeMarketplaceId { get; set; }

        public DateTime? FromReSaveDate { get; set; }

        public bool IsApproveMode { get; set; }

        public static StyleSearchFilterViewModel Empty
        {
            get { return new StyleSearchFilterViewModel(); } 
        }
    }
}