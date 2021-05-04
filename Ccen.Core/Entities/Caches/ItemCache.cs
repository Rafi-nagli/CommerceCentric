using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.Caches
{
    public class ItemCache : BaseCacheEntity
    {
        public DateTime? LastSoldDate { get; set; }
    }
}
