using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.DTO.Orders;

namespace Amazon.Core.Models.Dhl
{
    public class BookPickupInfo
    {
        public string ConfirmationNumber { get; set; }
        public DateTime? PickupDate { get; set; }
        public TimeSpan? ReadyByTime { get; set; }
        public TimeSpan? SecondReadyByTime { get; set; }
        public DateTime? NextPickupDate { get; set; }
        public decimal PickupCharge { get; set; }
        public TimeSpan? CallinTime { get; set; }
        public TimeSpan? SecondCallinTime { get; set; }

        public ScheduledPickupDTO ToDto()
        {
            return new ScheduledPickupDTO()
            {
                ConfirmationNumber = ConfirmationNumber,
                PickupDate = PickupDate,
                PickupCharge = PickupCharge,
                ReadyByTime = ReadyByTime,
                CallInTime = CallinTime,
            };
        }
    }
}
