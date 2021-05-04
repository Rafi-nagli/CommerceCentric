using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Amazon.Web.ViewModels.Messages
{
    public class MarkAsReadParams
    {
        public List<long> IdList { get; set; }
        public bool ForAll { get; set; }
        public bool ReadStatus { get; set; }

        public override string ToString()
        {
            return "IdList=" + (IdList != null ? String.Join(", ", IdList) : "[null]")
                   + ", ForAll=" + ForAll
                   + ", ReadStatus=" + ReadStatus;
        }
    }
}