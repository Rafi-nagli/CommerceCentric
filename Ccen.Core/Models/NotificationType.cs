using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models
{
    public enum NotificationType
    {
        None = 0,
        LabelNeverShipped = 100,
        LabelGotStuck = 101,
        AmazonProductImageChanged = 501,
    }
}
