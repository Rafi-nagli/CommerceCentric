using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.Events
{
    public class SaleEventFeed
    {
        [Key]
        public long Id { get; set; }
        public long SaleEventId { get; set; }
        public string Filename { get; set; }

        public DateTime CreateDate { get; set; }
        public long? CreatedBy { get; set; }
    }
}
