﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO
{
    public class ListingFBAEstFeeDTO : IReportItemDTO
    {
        public long Id { get; set; }

        public string SKU { get; set; }
        public string ASIN { get; set; }

        public decimal? YourPrice { get; set; }
        public decimal? SalesPrice { get; set; }


        public string Currency { get; set; }
        public decimal? EstimatedFee { get; set; }
        public decimal? EstimatedReferralFeePerUnit { get; set; }
        public decimal? EstimatedVariableClosingFee { get; set; }
        public decimal? EstimatedOrderHandlingFeePerOrder { get; set; }
        public decimal? EstimatedPickPackFeePerUnit { get; set; }
        public decimal? EstimatedWeightHandlingFeePerUnit { get; set; }

        public bool IsRemoved { get; set; }
        public DateTime? UpdateDate { get; set; }
        public DateTime? CreateDate { get; set; }
    }
}
