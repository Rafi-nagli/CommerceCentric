using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Common.ExcelExport
{
    public class ExcelColumnOverrideInfo
    {
        public int? Index { get; set; }
        public string Title { get; set; }
        
        public bool RemoveIt { get; set; }
    }
}
