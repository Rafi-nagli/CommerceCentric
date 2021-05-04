using System;
using System.ComponentModel.DataAnnotations;

namespace Amazon.Core.Entities
{
    public class TrackingOrder : BaseDateAndByEntity
    {
        [Key]
        public long Id { get; set; }
        //public long? OrderId { get; set; }

        public string Carrier { get; set; }
        public string TrackingNumber { get; set; }
        public string Comment { get; set; }
        public bool Deleted { get; set; }
    }
}
