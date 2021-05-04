using System;
using Amazon.Api;
using Amazon.Common.Helpers;
using Amazon.Core.Models;

namespace Amazon.Model.Implementation.Markets
{
    public class MarketUrlHelper
    {
        public static string GetTrackingUrl(string trackingNumber, string carrier, int? source = null)// = TrackingStatusSources.None)
        {
            if (string.IsNullOrEmpty(trackingNumber))
                return "";

            if (carrier == "USPS"
                || carrier == "IBC"
                || String.IsNullOrEmpty(carrier)) //TODO: Temporary
            {
                if (source == (int)TrackingStatusSources.CanadaPost)
                    return "https://www.canadapost.ca/cpotools/apps/track/personal/findByTrackNumber?trackingNumber=" + trackingNumber + "&LOCALE=en&LOCALE2=en";
                return "https://tools.usps.com/go/TrackConfirmAction?tLabels=" + trackingNumber;
            }
            if (carrier == "DHL" || carrier == "DHLMX")
                return String.Format("http://www.dhl.com/content/g0/en/express/tracking.shtml?AWB={0}&brand=DHL",
                    trackingNumber);
            if (carrier == "DYNAMEX")
                return String.Format("https://direct.dynamex.com/dxnow5/Track?trackingNumber={0}", trackingNumber);

            if (StringHelper.IsEqualNoCase(carrier, "FEDEX"))
                return String.Format("https://www.fedex.com/apps/fedextrack/?action=track&trackingnumber=" + trackingNumber);

            if (carrier == "UPS")
                return String.Format("https://wwwapps.ups.com/tracking/tracking.cgi?tracknum=" + trackingNumber);

            if (carrier == "SKYPOSTAL" || carrier == "CanadaPost")
                return String.Format("http://www.canadapost.ca/cpotools/apps/track/personal/findByTrackNumber?trackingNumber={0}&LOCALE=EN", trackingNumber);

            return "";
        }

        public static string GetSellarCentralInventoryUrl(string asin, MarketType market, string marketplaceId)
        {
            if (market == MarketType.Amazon)
            {
                return "https://sellercentral.amazon.com/myi/search/ProductSummary?asin=" + asin;
            }
            if (market == MarketType.AmazonEU)
            {
                return "https://sellercentral.amazon.co.uk/myi/search/ProductSummary?asin=" + asin;
            }
            if (market == MarketType.AmazonAU)
            {
                return "https://sellercentral.amazon.com.au/myi/search/ProductSummary?asin=" + asin;
            }
            if (market == MarketType.eBay)
            {
                //return "http://cgi.ebay.com/ws/eBayISAPI.dll?ViewItem&item=#=tmpl.index(tmpl.split(ASIN, "@(eBayUtils.CustomDelimeter) "), 0)#";
            }
            if (market == MarketType.Magento)
            {
                return "http://www.premiumapparel.com/index.php/admin/catalog_product/edit/id/" + asin;
            }
            if (market == MarketType.Walmart)
            {
                return "https://seller.walmart.com/items-and-inventory/manage-items";
            }
            if (market == MarketType.WalmartCA)
            {
                return "https://seller.walmart.ca/items-and-inventory/manage-items";
            }
            if (market == MarketType.Shopify)
            {
                return "#";
            }
            if (market == MarketType.WooCommerce)
            {
                return "#";
            }
            if (market == MarketType.Groupon)
            {
                return "#";
            }
            if (market == MarketType.OverStock)
            {
                return "#";
            }
            if (market == MarketType.Jet)
            {
                return "https://partner.jet.com/products";
            }
            return "#";
        }

        public static string GetSellarCentralOrderUrl(MarketType market, string marketplaceId, string orderId, string marketOrderId)
        {
            if (market == MarketType.Amazon)
            {
                return "https://sellercentral.amazon.com/orders-v3/order/" + orderId;
            }
            if (market == MarketType.AmazonEU)
            {
                return "https://sellercentral.amazon.co.uk/gp/orders-v2/details?orderID=" + orderId;
            }
            if (market == MarketType.AmazonAU)
            {
                return "https://sellercentral.amazon.com.au/gp/orders-v2/details?orderID=" + orderId;
            }
            if (market == MarketType.eBay)
            {
                return "http://k2b-bulk.ebay.com/ws/eBayISAPI.dll?EditSalesRecord&orderid=" + orderId;
            }
            if (market == MarketType.Magento)
            {
                return "http://www.premiumapparel.com/index.php/admin/sales_order/view/order_id/" + orderId;
            }
            if (market == MarketType.Walmart)
            {
                return "https://seller.walmart.com/order-management/details";
            }
            if (market == MarketType.WalmartCA)
            {
                return "https://seller.walmart.ca/order-management/details";
            }
            if (market == MarketType.Shopify)
            {
                return "#";
            }
            if (market == MarketType.WooCommerce)
            {
                return "#";
            }
            if (market == MarketType.Groupon)
            {
                return "https://scm.commerceinterface.com/orders/&orderid=" + orderId;
            }
            if (market == MarketType.OverStock)
            {
                return "#";
            }
            if (market == MarketType.Jet)
            {
                return "https://partner.jet.com/orders";
            }
            return "#";
        }

        public static string GetMarketUrl(string asin, MarketType market, string marketplaceId)
        {
            if (String.IsNullOrEmpty(asin))
                return String.Empty;

            if (market == MarketType.Amazon
                || market == MarketType.AmazonEU
                || market == MarketType.AmazonAU)
            {
                return AmazonUtils.GetProductMarketUrl(asin, market, marketplaceId);
            }

            if (market == MarketType.eBay)
            {
                var parts = asin.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 0)
                    return "http://cgi.ebay.com/ws/eBayISAPI.dll?ViewItem&amp;rd=1&amp;item=" + parts[0];
            }

            if (market == MarketType.Magento)
            {
                //http://www.premiumapparel.com/index.php/catalog/product/view/id/40
                return "http://www.premiumapparel.com/index.php/catalog/product/view/id/" + asin;
            }

            if (market == MarketType.Walmart)
            {
                return String.Format("https://www.walmart.com/ip/item/{0}", asin); //?portalSelectedSellerId=bf505621-095c-4d24-ab6c-e285e41a4151
            }

            if (market == MarketType.WalmartCA)
            {
                return String.Format("https://www.walmart.ca/ip/item/{0}", asin); //?portalSelectedSellerId=bf505621-095c-4d24-ab6c-e285e41a4151
            }

            if (market == MarketType.Shopify)
            {
                return "";
            }

            if (market == MarketType.WooCommerce)
            {
                return "";
            }

            if (market == MarketType.Groupon)
            {
                return "";
            }

            if (market == MarketType.OverStock)
            {
                return "https://www.overstock.com/" + asin + "/product.html";
            }

            if (market == MarketType.Jet)
            {
                return "";
            }

            return "";
        }
    }
}
