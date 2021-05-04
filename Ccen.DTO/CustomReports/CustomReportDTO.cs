using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.CustomReports
{
    public class CustomReportDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public bool InMenu { get; set; }

        public DateTime CreateDate { get; set; }
        public long? CreatedBy { get; set; }
    }
}
