using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models
{
    public enum BoxStatuses
    {
        Open = 0,
        Closed = 1
    }

    public class BoxStatusesHelper
    {
        public static string GetName(BoxStatuses status)
        {
            switch (status)
            {
                case BoxStatuses.Open:
                    return "Waiting";
                case BoxStatuses.Closed:
                    return "Closed";
            }
            return "-";
        }
    }
}
