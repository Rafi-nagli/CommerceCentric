using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core.Exports.Attributes;

namespace Amazon.InventoryUpdateManual.ExcelModel
{
    public class LocationResult
    {
        [ExcelSerializable("Style in our system", Order = 1, Width = 21)]
        public string StyleInOurSystem { get; set; }

        [ExcelSerializable("Suitable styles in excel", Order = 2, Width = 41)]
        public string SuitableStylesInExcel { get; set; }

        [ExcelSerializable("Excel Style", Order = 3, Width = 21)]
        public string ExcelStyle { get; set; }
        
        [ExcelSerializable("Locations", Order = 4, Width = 21)]
        public string Locations { get; set; }

        [ExcelSerializable("Comments", Order = 5, Width = 40)]
        public string Comments { get; set; }
    }
}
