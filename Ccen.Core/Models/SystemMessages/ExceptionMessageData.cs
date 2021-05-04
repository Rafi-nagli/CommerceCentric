using Amazon.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models.SystemMessages
{
    public class ExceptionMessageData : ISystemMessageData
    {
        public string Message { get; set; }
    }
}
