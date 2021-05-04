using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.Enums
{
    public enum ListingsModeType
    {
        All = 0,
        OnlyFBA = 1,
        WithoutFBA = 2
    }

    public static class ListingModeHelper
    {
        public static string GetName(ListingsModeType type)
        {
            switch (type)
            {
                case ListingsModeType.All:
                    return "All";
                case ListingsModeType.OnlyFBA:
                    return "Only FBA";
                case ListingsModeType.WithoutFBA:
                    return "w/o FBA";
            }
            return "-";
        }
    }
}
