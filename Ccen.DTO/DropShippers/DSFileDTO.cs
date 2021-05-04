using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.DropShippers
{
    public class DSFileDTO
    {
        public long Id { get; set; }
        public long DropShipperId { get; set; }
        public string DropShipperName { get; set; }

        public int? ProductType { get; set; }

        public string Filename { get; set; }
        public int FileType { get; set; }
        public DateTime ReceivedDate { get; set; }
        public int ProcessedStatus { get; set; }
        public int LinesTotal { get; set; }
        public int LinesProcessed { get; set; }
        public int LinesFailed { get; set; }
        public DateTime ProcessedDate { get; set; }

        //Additional
        public IList<DSFileLineDTO> Lines { get; set; }
        public IList<DSFileMessageDTO> Messages { get; set; }
    }
}
