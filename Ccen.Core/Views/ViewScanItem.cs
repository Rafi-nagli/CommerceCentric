using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Views
{
    public class ViewScanItem
    {
        [Key]
        public int Id { get; set; }

        public string Barcode { get; set; }
        public DateTime? CreateDate { get; set; }
    }
}
