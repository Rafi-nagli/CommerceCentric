using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.Inventory
{
    public class WalmartBrandInfo
    {
        [Key]
        public int Id { get; set; }
        public string Character { get; set; }
        public string Brand { get; set; }
        public string GlobalBrandLicense { get; set; }
    }
}
