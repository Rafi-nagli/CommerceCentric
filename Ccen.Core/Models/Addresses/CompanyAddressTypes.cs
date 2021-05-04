using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models
{
    public enum CompanyAddressTypes
    {
        /// <summary>
        /// PO BOX
        /// </summary>
        Default = 0, //PO BOX

        /// <summary>
        /// Pickup
        /// </summary>
        Physical = 1, //Pickup

        Groupon = 2,

        Canada = 3,
    }
}
