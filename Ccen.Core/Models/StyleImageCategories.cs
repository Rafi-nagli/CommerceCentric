using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models
{
    public enum StyleImageCategories
    {
        None = 0,
        Flat = 10,
        FlatMain = 11,
        Live = 20,
        LiveMain = 21,
        SizeChart = 30,
        Swatch = 40,
        Deleted = 1000,
    }

    public static class StyleImageCategoryHelper
    {
        public static string GetName(StyleImageCategories category)
        {
            switch (category)
            {
                case StyleImageCategories.None:
                    return "None";
                case StyleImageCategories.Flat:
                    return "Flat";
                case StyleImageCategories.FlatMain:
                    return "Flat Main";
                case StyleImageCategories.Live:
                    return "Live";
                case StyleImageCategories.LiveMain:
                    return "Live Main";
                case StyleImageCategories.SizeChart:
                    return "Size Chart";
                case StyleImageCategories.Swatch:
                    return "Swatch";
                case StyleImageCategories.Deleted:
                    return "Deleted";
            }
            return "n/a";
        }
    }
}
