using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Views
{
    public class ViewScanOrder
    {
        [Key]
        public int Id { get; set; }

        public string Description { get; set; }
        public DateTime OrderDate { get; set; }
        public bool IsFBA { get; set; }
        public string FileName { get; set; }
    }
}
