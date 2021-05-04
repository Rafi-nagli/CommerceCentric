using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models.Enums
{
    public enum AmazonFeedType
    {
        None = 0,
        OrderFulfillment = 1,
        OrderAcknowledgement = 2,
        OrderAdjustment = 3,
        Price = 4,
        Inventory = 5,
        Product = 6,
        ProductImage = 7,
        PriceRule = 8,
        Relationship = 9,
        ProductDelete = 10,
    }
}
