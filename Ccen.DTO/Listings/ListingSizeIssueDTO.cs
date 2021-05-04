using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Listings
{
    public class ListingSizeIssueDTO
    {
        public long Id { get; set; }

        public long ListingId { get; set; }
        public string AmazonSize { get; set; }
        public string StyleSize { get; set; }
        public bool IsActual { get; set; }

        public DateTime ReCheckDate { get; set; }
        public DateTime CreateDate { get; set; }

    }
}
