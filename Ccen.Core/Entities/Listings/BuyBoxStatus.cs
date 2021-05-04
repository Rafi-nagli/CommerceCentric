using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models;
using Amazon.DTO;

namespace Amazon.Core.Entities
{
    public class BuyBoxStatus
    {
        [Key]
        public long Id { get; set; }
        public string ASIN { get; set; }
        public string Barcode { get; set; }
        public int Market { get; set; }
        public string MarketplaceId { get; set; }

        public string WinnerMerchantName { get; set; }
        
        public decimal? WinnerPrice { get; set; }
        public DateTime? WinnerPriceLastChangeDate { get; set; }
        public decimal? WinnerPriceLastChangeValue { get; set; }


        public decimal? WinnerSalePrice { get; set; }
        public DateTime? WinnerSalePriceLastChangeDate { get; set; }
        public decimal? WinnerSalePriceLastChangeValue { get; set; }


        public decimal? WinnerAmountSaved { get; set; }

        public DateTime CheckedDate { get; set; }
        public DateTime? LostWinnerDate { get; set; }
        public BuyBoxStatusCode Status { get; set; }

        public bool IsIgnored { get; set; }
    }
}
