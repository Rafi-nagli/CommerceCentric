using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.DTO.Vendors;

namespace Amazon.Web.ViewModels.Vendors
{
    public class VendorOrderItemSizeViewModel
    {
        public long Id { get; set; }
        public string Size { get; set; }
        public int? Breakdown { get; set; }
        public string ASIN { get; set; }

        public int Order { get; set; }

        public DateTime? CreateDate { get; set; }

        public VendorOrderItemSizeViewModel()
        {
            
        }

        public VendorOrderItemSizeViewModel(VendorOrderItemSizeDTO vendorItemSizeDto)
        {
            Id = vendorItemSizeDto.Id;
            Size = vendorItemSizeDto.Size;
            Breakdown = vendorItemSizeDto.Breakdown;
            ASIN = vendorItemSizeDto.ASIN;
            Order = vendorItemSizeDto.Order;

            CreateDate = vendorItemSizeDto.CreateDate;
        }
    }
}