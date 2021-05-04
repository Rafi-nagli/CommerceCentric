using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Listings
{
    public class ItemErrorDTO
    {
        public string Type { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
    }
}
