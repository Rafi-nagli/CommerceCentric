using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.Orders
{
    public class OrderChangeHistory
    {
        [Key]
        public long Id { get; set; }

        public long OrderId { get; set; }
        public string FieldName { get; set; }
        public string FromValue { get; set; }
        public string ExtendFromValue { get; set; }
        public string ToValue { get; set; }
        public string ExtendToValue { get; set; }

        public long? ChangedBy { get; set; }
        public DateTime ChangeDate { get; set; }
    }
}
