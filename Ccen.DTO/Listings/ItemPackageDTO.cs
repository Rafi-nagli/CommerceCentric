using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Listings
{
    public class ItemPackageDTO
    {
        public decimal? PackageLength { get; set; }
        public decimal? PackageWidth { get; set; }
        public decimal? PackageHeight { get; set; }

        public decimal? MaxVolume { get; set; }

        public bool IsEmpty
        {
            get { return !PackageWidth.HasValue || !PackageHeight.HasValue || !PackageWidth.HasValue; }
        }
    }
}
