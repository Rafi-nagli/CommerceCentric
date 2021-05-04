using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models;
using Amazon.DTO;

namespace Amazon.Core.Entities
{
    public class SystemAction
    {
        [Key]
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
    }
}
