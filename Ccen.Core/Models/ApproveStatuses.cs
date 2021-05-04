using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models
{
    public enum ApproveStatuses
    {
        None = 0,
        Approved = 1,
        Declined = 2,
        Mixed = 3,
    }

    public static class ApproveStatusHelper
    {
        public static string GetName(ApproveStatuses status)
        {
            if (status == ApproveStatuses.Approved)
                return "Approved";
            if (status == ApproveStatuses.Declined)
                return "Declined";

            return "";
        }
    }
}
