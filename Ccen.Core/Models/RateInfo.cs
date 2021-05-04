using Amazon.DTO.Listings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models
{
    public class RateInfo
    {
        public int ServiceType { get; set; }
        public int PackageType { get; set; }

        public decimal MaxPackageWidth { get; set; }
        public decimal MaxPackageHeight { get; set; }
        public decimal MaxPackageLength { get; set; }
        public decimal? MaxPackageVolume { get; set; }

        public ItemPackageDTO PackageSize
        {
            get
            {
                return new ItemPackageDTO()
                {
                    PackageWidth = MaxPackageWidth,
                    PackageHeight = MaxPackageHeight,
                    PackageLength = MaxPackageLength,

                    MaxVolume = MaxPackageVolume,
                };
            }
        }
    }
}
