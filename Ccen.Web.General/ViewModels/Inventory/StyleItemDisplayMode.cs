using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Amazon.Web.ViewModels.Inventory
{
    public enum StyleItemDisplayMode
    {
        Standard = 1,
        StandardNoActions = 2,
        WithQuantity,
        BoxBreakdown,
        BoxQty
    }
}