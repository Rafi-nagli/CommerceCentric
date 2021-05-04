using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.Orders
{
    public class OrderSearch
    {
        [Key]
        public long Id { get; set; }
        public string OrderNumber { get; set; }
        public DateTime? CreateDate { get; set; }
        public long? CreatedBy { get; set; }
    }
}
