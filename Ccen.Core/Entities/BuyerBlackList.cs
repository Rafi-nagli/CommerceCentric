using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities
{
    public class BuyerBlackList : BaseDateAndByEntity
    {
        [Key]
        public long Id { get; set; }

        public string OrderId { get; set; }

        public string Reason { get; set; }

    }
}
