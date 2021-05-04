using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.Listings
{
    public class ListingSizeIssue
    {
        [Key]
        public long Id { get; set; }

        public long ListingId { get; set; }
        public string AmazonSize { get; set; }
        public string StyleSize { get; set; }
        public bool IsActual { get; set; }

        public DateTime ReCheckDate { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
