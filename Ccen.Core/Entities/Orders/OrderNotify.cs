using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.Orders
{
    public class OrderNotify
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public long OrderId { get; set; }
        public int Type { get; set; }
        public int Status { get; set; }
        public string Params { get; set; }
        public string Message { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
