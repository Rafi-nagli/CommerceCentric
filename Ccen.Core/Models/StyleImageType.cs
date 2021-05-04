using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models
{
    public enum StyleImageType
    {
        None = 0,
        Swatch = 5,
        LoRes = 10,
        MidRes = 12,
        HiRes = 15,
        Downloaded = 20,
        Unavailable = 404,
    }

    public static class StyleImageTypeHelper
    {
        public static string GetName(StyleImageType type)
        {
            switch (type)
            {
                case StyleImageType.None:
                    return "-";
                case StyleImageType.Unavailable:
                    return "Image unavailable";
                case StyleImageType.LoRes:
                    return "Low-res";
                case StyleImageType.MidRes:
                    return "Mid-res";
                case StyleImageType.Swatch:
                    return "Swatch";
                case StyleImageType.HiRes:
                    return "High-res";
                
            }
            return "-";
        }
    }
}
