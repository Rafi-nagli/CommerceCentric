using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Amazon.Common.Helpers;
using Amazon.DTO;

namespace Amazon.Common.Models
{
    public class CommercialInvoiceHelper
    {
        public static Byte[] BuildCommercialInvoice(string rootPath,
            string orderNumber,
            OrderShippingInfoDTO shippingInfo, 
            AddressDTO fromAddress, 
            AddressDTO toAddress,
            IList<DTOOrderItem> items,
            string trackingNumber,
            DateTime shipDate)
        {
            var html = File.ReadAllText(Path.Combine(rootPath, "commercial_invoice.html"));

            var weight = (decimal)shippingInfo.WeightD/(decimal) 16;

            var fromString = fromAddress.FullName + "<br/>"
                             + fromAddress.Address1 + "<br/>"
                             + fromAddress.City + ", " + fromAddress.State + " " + fromAddress.Zip + "<br/>"
                             + fromAddress.Country + "<br/>"
                             + fromAddress.Phone + "<br/>"
                             + fromAddress.BuyerEmail;

            var toString = toAddress.FinalFullName + "<br/>"
                           + toAddress.FinalAddress1 + "<br/>"
                           + (!String.IsNullOrEmpty(toAddress.FinalAddress2)
                               ? toAddress.FinalAddress2 + "<br/>"
                               : "")
                            + toAddress.FinalCity + ", " + toAddress.FinalState + " " + toAddress.FinalZip + "<br/>"
                            + toAddress.FinalCountry + "<br/>"
                            + toAddress.FinalPhone + "<br/>"
                            + toAddress.BuyerEmail;

            var currency = shippingInfo.TotalPriceCurrency;
            var totalPriceInUSD = PriceHelper.RougeConvertToUSD(currency, items.Sum(i => i.ItemPrice));
            
            var lines = String.Empty;
            foreach (var item in items)
            {
                var itemPriceInUSD = PriceHelper.RougeConvertToUSD(currency, item.ItemPrice/item.Quantity);
                var totalItemsPriceInUSD = PriceHelper.RougeConvertToUSD(currency, item.ItemPrice);

                lines += String.Format(
                    "<tr><td>Watches</td><td></td><td>US</td><td>{0}</td><td>EA</td><td>{1}</td><td>{2}</td></tr>",
                    item.Quantity,
                    itemPriceInUSD.ToString("0.00") + " USD", //currency,
                    totalItemsPriceInUSD.ToString("0.00") + " USD");// + currency);
            }

            html = html.Replace("{Date}", shipDate.ToString("yyyy-MM-dd"));
            html = html.Replace("{ShipperRef}", orderNumber);

            html = html.Replace("{ShipperName}", fromAddress.FullName);
            html = html.Replace("{ShipperAddress}", fromString);
            html = html.Replace("{FromContact}", fromAddress.ContactName);
            
            html = html.Replace("{ReceiverAddress}", toString);
            html = html.Replace("{DestinationCountry}", toAddress.FinalCountry);
            html = html.Replace("{ToContact}", toAddress.FinalFullName);
            html = html.Replace("{ConsigneeName}", toAddress.FinalFullName);

            html = html.Replace("{Weight}", weight.ToString("0.00"));
            html = html.Replace("{Waybill}", trackingNumber);
            html = html.Replace("{TotalAmount}", totalPriceInUSD.ToString("0.00"));
            html = html.Replace("{Currency}", "USD");//currency
            html = html.Replace("{Lines}", lines);

            var css = File.ReadAllText(Path.Combine(rootPath, "commercial_invoice.css")); ;// @".headline{font-size:200%} td { padding:20px }";

            return PdfHelper.BuildPdfFromHtml(html, css, 2);
        }
    }
}
