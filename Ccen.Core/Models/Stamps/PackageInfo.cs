using Amazon.DTO.Listings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models.Stamps
{
    public class PackageInfo
    {
        public ShippingTypeCode ServiceTypeUniversal { get; set; }
        public PackageTypeCode PackageTypeUniversal { get; set; }

        public string RequiredServiceIdentifier { get; set; }

        public double? Weight { get; set; }
        public decimal? PackageLength { get; set; }
        public decimal? PackageWidth { get; set; }
        public decimal? PackageHeight { get; set; }

        public IList<OrderItemRateInfo> Items;

        public int GroupId { get; set; }


        public ItemPackageDTO GetDimension()
        {
            return new ItemPackageDTO()
            {
                PackageLength = PackageLength,
                PackageWidth = PackageWidth,
                PackageHeight = PackageHeight
            };
        }
    }
}
