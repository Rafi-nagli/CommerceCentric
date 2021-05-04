using Amazon.Core.Models.Calls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Core.Models.Calls
{
    public class CallReqResResult<T> : CallResult<T>
    {
        public string RequestBody { get; set; }
        public string ResponseBody { get; set; }

        public static CallReqResResult<T> Success(T data)
        {
            return new CallReqResResult<T>()
            {
                Data = data,
                Status = CallStatus.Success
            };
        }

        public static CallReqResResult<T> Success(T data, string requestBody, string responseBody)
        {
            return new CallReqResResult<T>()
            {
                Data = data,
                RequestBody = requestBody,
                ResponseBody = responseBody,
                Status = CallStatus.Success
            };
        }

        public static CallReqResResult<T> Fail(string message, Exception ex, string requestBody, string responseBody)
        {
            return new CallReqResResult<T>()
            {
                Message = message,
                RequestBody = requestBody,
                ResponseBody = responseBody,
                Exception = ex,
                Status = CallStatus.Fail
            };
        }

        public static CallReqResResult<T> Fail(string message, Exception ex, string statusCode, string requestBody, string responseBody)
        {
            return new CallReqResResult<T>()
            {
                Message = message,
                Exception = ex,
                Status = CallStatus.Fail,
                StatusCode = statusCode,
                RequestBody = requestBody,
                ResponseBody = responseBody,
                Data = default(T)
            };
        }
    }
}
