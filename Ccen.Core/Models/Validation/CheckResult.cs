using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.DTO;

namespace Amazon.Core.Models.Validation
{
    public class CheckResult
    {
        public int Status { get; set; }
        public string Message { get; set; }

        public IList<string> AdditionalData { get; set; }

        public bool IsSuccess
        {
            get { return Status == 1; }
            set { Status = value ? 1 : 0; }
        }

        public override string ToString()
        {
            return "Status=" + Status + "\r\n" +
                   "Message=" + Message + "\r\n" +
                   "AdditionalData=" + (AdditionalData != null ? string.Join(", ", AdditionalData) : "[null]");
        }
    }
}
