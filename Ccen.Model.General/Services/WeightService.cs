using Amazon.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Core.Contracts;
using Amazon.DTO.Listings;
using Amazon.Common.Helpers;

namespace Amazon.Model.General
{
    public class WeightService : IWeightService
    {
        public double DefaultWeightForNoWeigth
        {
            get { return 10; }
        }

        public double ComposeOrderWeight(IList<ListingOrderDTO> items)
        {
            double weight = 0;
            var quantity = 0;
            foreach (var item in items)
            {
                weight += (item.Weight ?? 0) * item.QuantityOrdered;
                quantity += item.QuantityOrdered;
            }
            weight = AdjustWeight(weight, quantity);
            return PriceHelper.RoundToTwoPrecision(weight).Value;
        }

        public double AdjustWeight(double weight, int itemCount)
        {
            weight += 0.75;

            if (weight > 15.97 && weight < 16.99) //16.01
            {
                weight = 15.97;
            }
            var rounded = Math.Round(weight, MidpointRounding.ToEven);

            return rounded < weight ? rounded : weight;
        }

        public ItemPackageDTO ComposePackageSize(IList<ListingOrderDTO> items)
        {
            return ComposePackageSize(items.Select(i => new DTOOrderItem()
            {
                PackageWidth = i.PackageWidth,
                PackageHeight = i.PackageHeight,
                PackageLength = i.PackageLength,
                Quantity = i.QuantityOrdered,
            }).ToList());
        }

        public ItemPackageDTO ComposePackageSize(IList<DTOOrderItem> items)
        {
            var result = new ItemPackageDTO();
            if (items == null || !items.Any())
                return result;

            if (items.Any(i => i.PackageHeight == null
                || i.PackageWidth == null
                || i.PackageLength == null))
                return result;

            var packages = new List<ItemPackageDTO>();
            foreach (var item in items)
            {
                var package = new ItemPackageDTO()
                {
                    PackageLength = item.PackageLength ?? 1,
                    PackageWidth = item.PackageWidth ?? 1,
                    PackageHeight = item.PackageHeight ?? 1,
                };
                if (item.Quantity > 1)
                {
                    var minDimension = PriceHelper.Min(PriceHelper.Min(package.PackageLength, package.PackageWidth), package.PackageHeight);
                    if (package.PackageLength == minDimension)
                    {
                        package.PackageLength *= item.Quantity;
                    }
                    else if (package.PackageWidth == minDimension)
                    {
                        package.PackageWidth *= item.Quantity;
                    }
                    else if (package.PackageHeight == minDimension)
                    {
                        package.PackageHeight *= item.Quantity;
                    }
                }

                packages.Add(package);
            }

            result.PackageLength = packages.Max(p => p.PackageLength);
            result.PackageWidth = packages.Max(p => p.PackageWidth);
            result.PackageHeight = packages.Max(p => p.PackageHeight);

            return result;
        }
    }
}
