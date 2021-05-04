using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core.Models.Calls;

namespace Amazon.Core.Contracts
{
    public interface ICallResult
    {
        CallStatus Status { get; set; }
        string StatusCode { get; set; }

        string Message { get; set; }
        Exception Exception { get; set; }

        bool IsSuccess { get; }
        bool IsFail { get; }
    }
}
