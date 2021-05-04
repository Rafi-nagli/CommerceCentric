using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models
{
    public enum OnHoldModes
    {
        None = 0,
        OnListing = 1,
        OnStyle = 2,
        OnBoth = 3,
        OnListingOrStyle = 4,
        OnListingWithActiveStyle = 5
    }

    public static class OnHoldModeHelper
    {
        public static string GetName(OnHoldModes type)
        {
            switch (type)
            {
                case OnHoldModes.None:
                    return "None";
                case OnHoldModes.OnListing:
                    return "On Listing";
                case OnHoldModes.OnStyle:
                    return "On Style";
                case OnHoldModes.OnBoth:
                    return "On Both";
                case OnHoldModes.OnListingOrStyle:
                    return "On Listing Or Style";
                case OnHoldModes.OnListingWithActiveStyle:
                    return "On Listing (with active style)";
            }
            return "-";
        }
    }
}
