using System;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts.Parser;
using Amazon.Core.Entities;
using Amazon.DTO;

namespace Amazon.ReportParser.LineParser
{
    public class AFMShipmentLineParser : ILineParser
    {
    //    private const int OrderId = 0;
    //    private const int BuyerEmail = 4;
    //    private const int ShipAddress1 = 17;
    //    private const int ShipAddress2 = 18;
    //    //private const int ShipAddress3 = 19;
    //    private const int ShipCity = 20;
    //    private const int ShipState = 21;
    //    private const int ShipPostalCode = 22;
    //    private const int ShipCountry = 23;
    //    private const int ShipPhoneNumber = 24;


    //    //private const int OrderItemId = 1;
    //    //private const int PurchaseDate = 2;
    //    //private const int PaymentsDate = 3;
    //    //private const int BuyerName = 5;
    //    //private const int BuyerPhoneNumber = 6;
    //    //private const int SKU = 7;
    //    //private const int ProductName = 8;
    //    //private const int QuantityPurchased = 9;
    //    //private const int Currency = 10;
    //    //private const int ItemPrice = 11;
    //    //private const int ItemTax = 12;
    //    //private const int ShippingPrice = 13;
    //    //private const int ShippingTax = 14;
    //    //private const int ShipServiceLevel = 15;
    //    //private const int RecipientName = 16;

    //    private const int Length = 66;
    //    //private const int
    //    //private const int
    //    //private const int
    //    //private const int

        public IReportItemDTO Parse(string[] fields, string[] headers)
        {
    //        string[] fields = line.Split('	');
    //        if (fields.Length < Length)
    //        {
    //            return null;
    //        }
            var order = new ItemDTO();
    //        for (var i = 0; i < fields.Length; i++)
    //        {
    //            try
    //            {
    //                var val = fields[i];
    //                switch (i)
    //                {
    //                    case OrderId:
    //                        order.OrderId = val;
    //                        break;
    //                    case BuyerEmail:
    //                        order.OrderEmail = val;
    //                        break;
    //                    case ShipAddress1:
    //                        order.ShippingAddress1 = val;
    //                        break;
    //                    case ShipAddress2:
    //                        order.ShippingAddress2 = val;
    //                        break;
    //                    case ShipCity:
    //                        order.ShippingCity = val;
    //                        break;
    //                    case ShipState:
    //                        order.ShippingState = val;
    //                        break;
    //                    case ShipPostalCode:
    //                        order.ShippingZip = ShippingUtils.GetZip(val);
    //                        order.ShippingZipAddon = ShippingUtils.GetZipAddon(val);
    //                        break;
    //                    case ShipCountry:
    //                        order.ShippingCountry = val;
    //                        break;
    //                    case ShipPhoneNumber:
    //                        order.ShippingPhone = val;
    //                        break;
    //                }
    //            }
    //            catch (Exception ex)
    //            {
    //                log.Error(string.Format("Unable to parse line: {0}", line), ex);
    //            }
    //        }
            return order;

        }
    }
}
