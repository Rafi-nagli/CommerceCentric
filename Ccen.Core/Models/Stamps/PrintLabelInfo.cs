
using System;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models.Stamps;

namespace Amazon.Core.Models
{
    public class PrintLabelInfo
    {
        public LabelPrintStatus PrintResult { get; set; }

        public string Image { get; set; }
        public string RelativeImagePath { get; set; }

        public string OrderId { get; set; }
        public string PersonName { get; set; }
        public int Number { get; set; }
        public int BatchId { get; set; }
        public bool Duplicate { get; set; }
        
        public ShippingTypeCode ServiceType { get; set; }
        public PackageTypeCode PackageType { get; set; }
        
        public int ShippingMethodId { get; set; }
        public string PackageNameOnLabel { get; set; }

        public PrintLabelSizeType LabelSize { get; set; }
        public int? RotationAngle { get; set; }

        public LabelSpecialType SpecialType { get; set; }

        public string Notes { get; set; }

        public bool IsPdf
        {
            get
            {
                if (!String.IsNullOrEmpty(Image) && Image.EndsWith(".pdf"))
                    return true;
                return false;
            }
        }

        public bool IsZpl
        {
            get
            {
                if (!String.IsNullOrEmpty(Image) && Image.EndsWith(".zpl"))
                    return true;
                return false;
            }
        }
    }
}
