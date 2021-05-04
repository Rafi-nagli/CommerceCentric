using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models
{
    public enum MarketType
    {
        None = 0,
        Amazon = 1,        
        eBay = 2,
        AmazonEU = 3,
        Magento = 4,
        Walmart = 5,
        Shopify = 6,
        Jet = 7,
        WalmartCA = 8,
        OverStock = 9,
        AmazonAU = 10,
        AmazonPrime = 11, //Virtual Market
        Groupon = 12,
        AmazonIN = 14,
        WooCommerce = 15,
        DropShipper = 100,
        CustomAshford = 101,
        OfflineOrders = 201,
        FtpMarket = 202,
    }
}
