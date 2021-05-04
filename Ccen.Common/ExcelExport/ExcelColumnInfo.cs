using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Amazon.Common.ExcelExport
{
    public class ExcelColumnInfo
    {
        public string Title { get; set; }
        public int Width { get; set; }
        public string Format { get; set; }
        public PropertyInfo Property { get; set; }
    }
}
