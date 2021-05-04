using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models.DropShippers
{
    public enum DSItemPublishStatuses
    {
        None = 0,
        Published = 1,
        NotPublishedOwnerHad = 10,
        NotPublishedOtherDSPrice = 11,
    }
}
