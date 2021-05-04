using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Orders
{
    public class PaymentInfoDTO
    {
        public string Recommendation { get; set; }
        public string Score { get; set; }

        public int PaymentValidationStatuses { get; set; }
    }
}
