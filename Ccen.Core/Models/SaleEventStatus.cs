using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models
{
    public enum SaleEventStatus
    {
        None = 0,
        Awaiting = 1,
        Launched = 5,
        Closed = 10
    }

    public static class SaleEventStatusHelper
    {
        public static string ToString(SaleEventStatus status)
        {
            switch (status)
            {
                case SaleEventStatus.Awaiting:
                    return "Awaiting";
                case SaleEventStatus.Closed:
                    return "Closed";
                case SaleEventStatus.Launched:
                    return "Launched";
            }
            return "n/a";
        }
    }
}
