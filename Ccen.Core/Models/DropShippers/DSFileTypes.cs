using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models.DropShippers
{
    public enum DSFileTypes
    {
        ItemsFull = 1,
        ItemsLite = 2,
        Fulfilment = 10,
    }

    public static class DSFileTypeHelper
    {
        public static string GetName(DSFileTypes type)
        {
            switch (type)
            {
                case DSFileTypes.ItemsFull:
                    return "Items";
                case DSFileTypes.ItemsLite:
                    return "Inventory";
            }
            return "";
        }
    }
}
