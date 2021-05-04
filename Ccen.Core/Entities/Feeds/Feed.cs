using System;
using System.ComponentModel.DataAnnotations;

namespace Amazon.Core.Entities
{
    public class Feed
    {
        [Key]
        public long Id { get; set; }
        public string AmazonIdentifier { get; set; }

        public int Market { get; set; }
        public string MarketplaceId { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }

        public string RequestFilename { get; set; }
        public string ResponseFilename { get; set; }
        
        public int MessageCount { get; set; }
        public DateTime SubmitDate { get; set; }
        //public bool IsProcessed { get; set; }
        //public bool Deleted { get; set; }

        public int Status { get; set; }

        public int Type { get; set; }
    }
}
