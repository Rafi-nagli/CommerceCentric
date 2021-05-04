using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.Enums
{
    public enum PriceChangeSourceType
    {
        None = 0,
        Initial = 5,
        ParentItemOffset = 10,
        EnterNewPrice = 20,
        SetByAutoPrice = 100,
        FromSaleFeed = 150,
    }


    public static class PriceChangeSourceTypeHelper
    {
        public static string GetName(PriceChangeSourceType type)
        {
            switch (type)
            {
                case PriceChangeSourceType.EnterNewPrice:
                    return "Enter New Price";
                case PriceChangeSourceType.FromSaleFeed:
                    return "From Sale Feed";
                case PriceChangeSourceType.Initial:
                    return "Initial";
                case PriceChangeSourceType.ParentItemOffset:
                    return "Parent Item Offset";
                case PriceChangeSourceType.SetByAutoPrice:
                    return "Set By Auto Price";

                default:
                    return type.ToString();
            }
            return "-";
        }
    }
}
