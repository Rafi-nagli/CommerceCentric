using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Amazon.Api.Exports;

namespace Amazon.Web.ViewModels.Inventory
{
    public class SizeViewModel
    {
        public long? StyleItemId { get; set; }

        public string Name { get; set; }

        public int? SizeId { get; set; }
        public int? SizeGroupId { get; set; }
        
        public string SizeGroupName { get; set; }

        public bool DefaultIsChecked { get; set; }

        public bool IsChecked { get; set; }
        public List<Barcode> Barcodes { get; set; }
        public ExportSizeGroup SizeGroupType { get; set; }

        [Range(0.0d, 10000.0d, ErrorMessage = "Please enter weight greater than 0")]
        public double? Weight { get; set; }
        public int? Quantity { get; set; }
    }
}