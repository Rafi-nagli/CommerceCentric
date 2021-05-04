using Amazon.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models.Calls
{
    public class CallResult : ICallResult
    {
        public CallStatus Status { get; set; }
        public string StatusCode { get; set; }
        public string Message { get; set; }
        public Exception Exception { get; set; }

        public bool IsSuccess
        {
            get { return Status == CallStatus.Success; }
        }

        public bool IsFail
        {
            get { return Status == CallStatus.Fail; }
        }

        public static CallResult Fail(string message, Exception ex)
        {
            return new CallResult()
            {
                Status = CallStatus.Fail,
                Exception = ex,
                Message = message
            };
        }

        public static CallResult Success()
        {
            return new CallResult()
            {
                Status = CallStatus.Success,
            };
        }
    }
}
