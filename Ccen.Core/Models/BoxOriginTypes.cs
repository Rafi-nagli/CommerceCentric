using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models
{
    public enum BoxOriginTypes
    {
        None = 0,
        ByUser = 1,
        ReInventorization = 5,
        VendorInvoice = 20
    }
}
