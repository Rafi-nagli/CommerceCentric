using Amazon.Core.Models.SystemActions.SystemActionDatas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Common.Helpers
{
    public class RefundHelper
    {
        public static decimal? GetAmount(ReturnOrderInput input)
        {
            return input.RefundAmount //NOTE: new way
                ?? (input.Items != null ?
                input.Items.Sum(i => i.RefundItemPrice
                    + (input.IncludeShipping ? i.RefundShippingPrice : 0)
                    - (input.DeductShipping ? i.DeductShippingPrice : 0)
                    - (input.IsDeductPrepaidLabelCost ? i.DeductPrepaidLabelCost : 0))
                : (decimal?)null);
        }
    }
}
