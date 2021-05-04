using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Users
{
    public class SQSAccountDTO
    {
        public long Id { get; set; }

        public long CompanyId { get; set; }

        public int Type { get; set; }

        public string EndPointUrl { get; set; }
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }

        public DateTime CreateDate { get; set; }
    }
}
