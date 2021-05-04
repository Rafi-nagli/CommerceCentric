using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Caches
{
    public class ItemCacheDTO : BaseCacheDto
    {
        public DateTime? LastSoldDate { get; set; }
    }
}
