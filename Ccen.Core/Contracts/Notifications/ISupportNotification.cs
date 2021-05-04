using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Contracts.Notifications
{
    public interface ISupportNotification
    {
        string Name { get; }

        IList<TimeSpan> When { get; }

        void Check();
    }
}
