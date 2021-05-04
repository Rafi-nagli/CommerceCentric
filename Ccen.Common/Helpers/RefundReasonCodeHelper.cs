using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core.Models;

namespace Amazon.Common.Helpers
{
    public class RefundReasonCodeHelper
    {
        public static string GetName(RefundReasonCodes code)
        {
            switch (code)
            {
                case RefundReasonCodes.Oversold:
                    return "Oversold";
                case RefundReasonCodes.Late:
                    return "Late";
                case RefundReasonCodes.Lost:
                    return "Lost";
                case RefundReasonCodes.Defective:
                    return "Defective";
                case RefundReasonCodes.Courtesy:
                    return "Courtesy";
                case RefundReasonCodes.Return:
                    return "Return";
                case RefundReasonCodes.Other:
                    return "Other";
            }

            return "";
        }
    }
}
