using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.Enums
{
    public enum FulfillmentChannelType
    {
        AFN = 0,
        MFN = 1,
    }

    public class FulfillmentChannelTypeEx
    {
        public static string AFN = "AFN";
        public static string MFN = "MFN";
    }
}
