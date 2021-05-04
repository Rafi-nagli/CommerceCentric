using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO
{
    public class SystemActionDTO
    {
        public long Id { get; set; }

        public long? ParentId { get; set; }

        public string GroupId { get; set; }
        public int Type { get; set; }
        public string Tag { get; set; }

        public string InputData { get; set; }
        public string OutputData { get; set; }

        public int Status { get; set; }

        public DateTime? AttemptDate { get; set; }
        public int AttemptNumber { get; set; }
        public DateTime CreateDate { get; set; }
        public long? CreatedBy { get; set; }

        public string CreatedByName { get; set; }
    }
}
