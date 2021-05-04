using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models.Enums
{
    public enum CountingStatuses
    {
        None = 0,
        Reviewed = 1,
        Counted = 2,
        Lost = 3,
        Recount = 4,
        Verified = 5
    }

    public static class CountingStatusesEx
    {
        public const string None = "None";
        public const string Reviewed = "Reviewed";
        public const string Counted = "Counted";
        public const string Lost = "Lost";
        public const string Recount = "Recount";
        public const string Verified = "Verified";
    }
}
