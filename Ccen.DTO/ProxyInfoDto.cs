using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO
{
    public class ProxyInfoDto
    {
        public long Id { get; set; }

        public string IPAddress { get; set; }

        public int Port { get; set; }

        public int SucceedRequestCount { get; set; }
        public DateTime? LastSucceedRequestDate { get; set; }

        public int FailedRequestCount { get; set; }
        public DateTime LastFailedRequestDate { get; set; }

        public int UseType { get; set; }

        public bool IsActive { get; set; }
    }
}
