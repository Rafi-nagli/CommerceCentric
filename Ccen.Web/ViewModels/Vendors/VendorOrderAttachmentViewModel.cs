using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.DTO.Vendors;

namespace Amazon.Web.ViewModels.Vendors
{
    public class VendorOrderAttachmentViewModel
    {
        public long Id { get; set; }
        public string FileName { get; set; }

        public VendorOrderAttachmentViewModel(VendorOrderAttachmentDTO vendorAttachmentDto)
        {
            Id = vendorAttachmentDto.Id;
            FileName = vendorAttachmentDto.FileName;
        }
    }
}