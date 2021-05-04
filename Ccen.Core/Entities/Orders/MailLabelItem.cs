using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.Orders
{
    public class MailLabelItem
    {
        [Key]
        public long Id { get; set; }

        public long MailLabelInfoId { get; set; }
        public long OrderId { get; set; }
        public string ItemOrderIdentifier { get; set; }
        public long StyleId { get; set; }
        public long StyleItemId { get; set; }
        public int Quantity { get; set; }
        public DateTime CreateDate { get; set; }
        public long? CreatedBy { get; set; }
    }
}
