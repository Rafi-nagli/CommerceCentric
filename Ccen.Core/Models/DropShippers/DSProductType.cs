using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models.DropShippers
{
    public enum DSProductType
    {
        Watches = 1,
        Sunglasses = 2,
        Jewelry = 3,
        WritingInstruments = 10,
    }

    public static class DSProductTypeHelper
    {
        public static string GetName(DSProductType type)
        {
            switch (type)
            {
                case DSProductType.Watches:
                    return "Watches";
                case DSProductType.Jewelry:
                    return "Jewelry";
                case DSProductType.Sunglasses:
                    return "Sunglasses";
                case DSProductType.WritingInstruments:
                    return "Writing Instruments";
            }
            return "";
        }
    }
}
