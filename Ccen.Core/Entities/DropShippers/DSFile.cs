using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.DropShippers
{
    public class DSFile
    {
        [Key]
        public long Id { get; set; }
        public long DropShipperId { get; set; }
        public string Filename { get; set; }
        public int FileType { get; set; }

        public DateTime ReceivedDate { get; set; }
        public int ProcessedStatus { get; set; }
        public int LinesTotal { get; set; }
        public int LinesProcessed { get; set; }
        public int LinesFailed { get; set; }
        public DateTime ProcessedDate { get; set; }
    }
}
