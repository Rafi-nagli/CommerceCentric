using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Orders
{
    public class OrderValidationDTO
    {
        public string CaseId { get; set; }

        public string VerificationStatus { get; set; }
        public bool GuaranteeEligible { get; set; }

        public decimal? Score { get; set; }

        public string MagentoStatus { get; set; }

        public DateTime? UpdateDate { get; set; }

        public DateTime? CreateDate { get; set; }
    }
}
