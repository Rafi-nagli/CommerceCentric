using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Sizes
{
    public class PackingSlipSizeMappingDTO
    {
        public int Id { get; set; }
        public string StyleString { get; set; }
        public string SourceSize { get; set; }
        public string DisplaySize { get; set; }
    }
}
