using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models
{
    public enum EmailResponseStatusEnum
    {
        None = 0,
        ReceivedFromMailbox = 1,
        Sent = 10,
        NoResponseNeeded = 15,
        ResponsePromised = 20,
    }
}
