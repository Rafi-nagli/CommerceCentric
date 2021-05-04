using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Caches
{
    public class BaseCacheDto
    {
        public long Id { get; set; }

        public bool IsDirty { get; set; }

        public DateTime? UpdateDate { get; set; }
        public DateTime? CreateDate { get; set; }
    }
}
