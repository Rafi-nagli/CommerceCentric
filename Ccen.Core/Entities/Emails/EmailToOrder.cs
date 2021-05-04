using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.Emails
{
    public class EmailToOrder : BaseDateAndByEntity
    {
        [Key]
        public long Id { get; set; }
        public long EmailId { get; set; }
        public string OrderId { get; set; }
    }
}
