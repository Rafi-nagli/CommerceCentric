using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Contracts.Cache
{
    public interface IDbCacheableEntry
    {
        long Key { get; }
        bool Deleted { get; }
    }
}
