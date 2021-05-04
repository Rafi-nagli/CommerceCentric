using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models
{
    public enum ProductAvailability
    {
        All = 1,
        InStock = 2,
    }

    public static class ProductAvailabilityHelper
    {
        public static string GetName(ProductAvailability type)
        {
            switch (type)
            {
                case ProductAvailability.All:
                    return "All";
                case ProductAvailability.InStock:
                    return "In Stock";
            }
            return "-";
        }
    }
}
