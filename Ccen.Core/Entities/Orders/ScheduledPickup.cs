using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.Orders
{
    public class ScheduledPickup : BaseDateAndByEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int ProviderType { get; set; }

        public DateTime RequestPickupDate { get; set; }
        public TimeSpan RequestReadyByTime { get; set; }
        public TimeSpan RequestCloseTime { get; set; }

        public string ResultMessage { get; set; }
        public DateTime? SendRequestDate { get; set; }
        
        public string ConfirmationNumber { get; set; }
        public DateTime? PickupDate { get; set; }
        public TimeSpan? ReadyByTime { get; set; }
        public decimal? PickupCharge { get; set; }
        public TimeSpan? CallInTime { get; set; }
    }
}
