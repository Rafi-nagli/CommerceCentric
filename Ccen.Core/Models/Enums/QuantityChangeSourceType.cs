using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.Enums
{
    public enum QuantityChangeSourceType
    {
        None = 0,
        Initial = 5,
        
        NewOrder = 10,

        OrderCancelled = 11,
        MailPage = 20,

        EnterNewQuantity = 50,

        UseBoxQuantity = 55,
        UseManuallyQuantity = 56,
        AddNewBox = 60,
        ChangeBox = 65,
        RemoveBox = 70,

        ArchiveBox = 72,
        UnArchiveBox= 73,

        ClosePreorderBox = 75,
        OpenPreorderBox = 76,

        AddSpecialCase = 80,
        RemoveSpecialCase = 81,

        SentToStore = 90,
        SentToFBA = 95,

        SetByAutoQuantity = 100, //Only for listing

        RemainingChanged = 110,

        OnHold = 85,
               

        SaleEventOnHold = 100,
        SaleEventOnUnHold = 101,
        SaleEventPurchased = 105,

        Photoshoot = 120,

        Dropshipper = 130,

        Removed = 1000,

        API = 2000,
    }

    public static class QuantityChangeSourceTypeHelper
    {
        public static string GetName(QuantityChangeSourceType type)
        {
            switch (type)
            {
                case QuantityChangeSourceType.AddNewBox:
                    return "Add Box";
                case QuantityChangeSourceType.RemoveBox:
                    return "Remove Box";
                case QuantityChangeSourceType.ChangeBox:
                    return "Change Box Qty";
                case QuantityChangeSourceType.UseBoxQuantity:
                    return "Switch to Box Qty";
                case QuantityChangeSourceType.UseManuallyQuantity:
                    return "Switch to Manually";

                case QuantityChangeSourceType.AddSpecialCase:
                    return "Special Case";
                case QuantityChangeSourceType.RemoveSpecialCase:
                    return "Special Case";

                case QuantityChangeSourceType.SentToFBA:
                    return "Sent to FBA";
                case QuantityChangeSourceType.SentToStore:
                    return "Sent to Store";

                case QuantityChangeSourceType.Initial:
                    return "Manually";

                case QuantityChangeSourceType.EnterNewQuantity:
                    return "Manually";

                case QuantityChangeSourceType.OnHold:
                    return "On Hold";
                case QuantityChangeSourceType.NewOrder:
                    return "New Order";
                case QuantityChangeSourceType.OrderCancelled:
                    return "Order Canceled";

                case QuantityChangeSourceType.RemainingChanged:
                    return "Snapshot";

                case QuantityChangeSourceType.SaleEventOnHold:
                    return "Sale Event (OnHold)";

                case QuantityChangeSourceType.SaleEventPurchased:
                    return "Sale Event (Purchased)";

                default:
                    return type.ToString();
            }
            return "-";
        }
    }
}
