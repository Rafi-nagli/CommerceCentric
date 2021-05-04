using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccen.Core.Entities.TrackingNumbers
{
    public class CustomTrackingNumber
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public string TrackingNumber { get; set; }

        public long? AttachedToShippingInfoId { get; set; }
        public string AttachedToTrackingNumber { get; set; }

        public DateTime? AttachedDate { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
