using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Sizes
{
    public class SizeMappingDTO
    {
        public int Id { get; set; }
        public string StyleSize { get; set; }
        public string ItemSize { get; set; }
        public int Priority { get; set; }
        public bool IsSystem { get; set; }
        public DateTime? UpdateDate { get; set; }
        public DateTime? CreateDate { get; set; }
    }
}
