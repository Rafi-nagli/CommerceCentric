using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models.Calls
{
    public class CallSearchResult<T> : CallResult<T>
    {
        public int Total { get; set; }
        public int Page { get; set; }

        public static CallSearchResult<T> Success(T data, int total)
        {
            return new CallSearchResult<T>()
            {
                Data = data,
                Total = total,
                Status = CallStatus.Success
            };
        }

        public new static CallSearchResult<T> Fail(string message, Exception ex)
        {
            return new CallSearchResult<T>()
            {
                Message = message,
                Exception = ex,
                Status = CallStatus.Fail
            };
        }
    }
}
