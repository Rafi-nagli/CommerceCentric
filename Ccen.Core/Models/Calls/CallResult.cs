using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core.Contracts;

namespace Amazon.Core.Models.Calls
{
    public class CallResult<T> : ICallResult
    {
        public CallStatus Status { get; set; }
        public string StatusCode { get; set; }

        public string Message { get; set; }

        public Exception Exception { get; set; }
        public T Data { get; set; }

        public bool IsSuccess
        {
            get { return Status == CallStatus.Success; }
        }

        public bool IsFail
        {
            get { return Status == CallStatus.Fail; }
        }

        public static CallResult<T> Success(T data)
        {
            return new CallResult<T>()
            {
                Data = data,
                Status = CallStatus.Success
            };
        }

        public static CallResult<T> Fail(string message, Exception ex)
        {
            return new CallResult<T>()
            {
                Message = message,
                Exception = ex,
                Status = CallStatus.Fail
            };
        }

        public static CallResult<T> Fail(string message, Exception ex, string statusCode)
        {
            return new CallResult<T>()
            {
                Message = message,
                Exception = ex,
                Status = CallStatus.Fail,
                StatusCode = statusCode,
                Data = default(T)
            };
        }
    }
}
