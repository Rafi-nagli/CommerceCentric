using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Shippings
{
    public class DhlInvoiceDTO
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public int Status { get; set; }

        public DateTime InvoiceDate { get; set; }
        public string InvoiceNumber { get; set; }

        public string Dimensions { get; set; }

        public string BillNumber { get; set; }
        public decimal ChargedBase { get; set; }
        public decimal ChargedSummary { get; set; }
        public decimal ChargedCredit { get; set; }

        public decimal? Estimated { get; set; }

        public string SourceFile { get; set; }

        public DateTime CreateDate { get; set; }

        //Additionals
        public string Country { get; set; }
        public string RateCode { get; set; }
    }
}
