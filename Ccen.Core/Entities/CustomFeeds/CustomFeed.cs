using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.DropShippers
{
    public class CustomFeed
    {
        [Key]
        public long Id { get; set; }
        public string FeedName { get; set; }
        public string ExportFileType { get; set; }
        public string ExportFileName { get; set; }

        public long? DropShipperId { get; set; }
        public int? OverrideDSFeedType { get; set; }
        public int? OverrideDSProductType { get; set; }

        public string Protocol { get; set; }
        public string FtpSite { get; set; }
        public string FtpFolder { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool IsPassiveMode { get; set; }
        public bool IsSFTP { get; set; }

        public DateTime? UpdateDate { get; set; }
        public DateTime CreateDate { get; set; }
        public long? CreatedBy { get; set; }
    }
}
