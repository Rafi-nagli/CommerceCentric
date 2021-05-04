using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.Sizes
{
    public class PackingSlipSizeMapping
    {
        public int Id { get; set; }
        public string SourceSize { get; set; }
        public string DisplaySize { get; set; }

        public DateTime CreateDate { get; set; }
        public long? CreatedBy { get; set; }
    }
}
