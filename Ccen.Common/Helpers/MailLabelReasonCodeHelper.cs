using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core.Models;

namespace Amazon.Common.Helpers
{
    public class MailLabelReasonCodeHelper
    {
        public static string GetName(MailLabelReasonCodes code)
        {
            switch (code)
            {
                case MailLabelReasonCodes.ReplacementLabelCode:
                    return "Replacement label";
                case MailLabelReasonCodes.ReplacingLostDamagedReasonCode:
                    return "Replacing Lost/Damaged";
                case MailLabelReasonCodes.ResendingOrderCode:
                    return "Resending order";
                case MailLabelReasonCodes.ExchangeCode:
                    return "Exchange";
                case MailLabelReasonCodes.ReturnLabelReasonCode:
                    return "Return Label";
                case MailLabelReasonCodes.ManualLabelCode:
                    return "Manual label";
                //new KeyValuePair<string, int?>("Fixed Address", 3),
                //new KeyValuePair<string, int?>("Change of Address", 6),
                //new KeyValuePair<string, int?>("Defective", 7),
                case MailLabelReasonCodes.OtherCode:
                    return "Other";
            }

            return "";
        }
    }
}
