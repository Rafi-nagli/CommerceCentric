using Amazon.DTO.Listings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Helpers
{
    public class PackageHelper
    {
        public static bool Is1x1x1(ItemPackageDTO package)
        {
            if (!package.PackageHeight.HasValue
                || !package.PackageLength.HasValue
                || !package.PackageWidth.HasValue)
                return true;

            if (package.PackageHeight == 1
                && package.PackageLength == 1
                && package.PackageWidth == 1)
                return true;

            return false;
        }


        public static bool Is2into1(ItemPackageDTO package1, ItemPackageDTO package2)
        {
            if (!package1.PackageHeight.HasValue
                || !package1.PackageLength.HasValue
                || !package1.PackageWidth.HasValue
                || !package2.PackageHeight.HasValue
                || !package2.PackageLength.HasValue
                || !package2.PackageWidth.HasValue)
                return true;

            var size1 = new List<decimal>() { package1.PackageLength.Value, package1.PackageWidth.Value, package1.PackageHeight.Value };
            var size2 = new List<decimal>() { package2.PackageLength.Value, package2.PackageWidth.Value, package2.PackageHeight.Value };
            size1.Sort();
            size2.Sort();

            var isIn = true;
            for (var i = 0; i < 3; i++)
            {
                isIn = isIn && size1[i] >= size2[i];
            }

            return isIn;
        }

        public static ItemPackageDTO FedexPak = new ItemPackageDTO()
        {
            PackageLength = 12M,
            PackageWidth = 15.5M,
            PackageHeight = 0.5M,
        };

        public static ItemPackageDTO FedexEnvelope = new ItemPackageDTO()
        {
            PackageLength = 9.5M,
            PackageWidth = 15.5M,
            PackageHeight = 0.5M,
        };
    }
}
