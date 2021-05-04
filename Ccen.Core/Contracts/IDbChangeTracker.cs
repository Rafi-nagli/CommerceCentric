using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Contracts
{
    public interface IDbChangeTracker
    {
        void TrackChanges(DateTime? when, long? by);
    }
}
