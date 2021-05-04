using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models
{
    public enum CommentType
    {
        None = 0,
        Address = 5,
        Notification = 10,
        IncomingEmail = 12,
        OutputEmail = 15,
        ReturnExchange = 20,
        MarketplaceClaim = 25,
        System = 40,
        Other = 50,
    }
}
