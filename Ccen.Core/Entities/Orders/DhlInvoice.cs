using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.Orders
{
    public class DhlInvoice
    {
        [Key]
        public int Id { get; set; }

        public string OrderNumber { get; set; }
        public int Status { get; set; }

        public DateTime InvoiceDate { get; set; }
        public string InvoiceNumber { get; set; }

        public string BillNumber { get; set; }
        public string Dimensions { get; set; }
        public decimal ChargedBase { get; set; }
        public decimal ChargedSummary { get; set; }
        public decimal ChargedCredit { get; set; }

        public decimal? Estimated { get; set; }

        public string SourceFile { get; set; }

        public DateTime? UpdateDate { get; set; }
        public long? UpdatedBy { get; set; }

        public DateTime CreateDate { get; set; }
    }
}
