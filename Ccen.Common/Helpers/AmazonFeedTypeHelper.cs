using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Core.Models;
using Amazon.Core.Models.Enums;

namespace Amazon.Common.Helpers
{
    public class AmazonFeedTypeHelper
    {
        public static string ToString(AmazonFeedType type)
        {
            switch (type)
            {
                case AmazonFeedType.Product:
                    return "Product";
                case AmazonFeedType.Inventory:
                    return "Inventory";
                case AmazonFeedType.Price:
                    return "Price";
            }
            return "-";
        }
    }
}
