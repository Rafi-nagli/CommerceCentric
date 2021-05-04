using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models.Calls
{
    public class CallHelper
    {
        public static void ThrowIfFail<T>(CallResult<T> result, Exception ex)
        {
            if (result.Status == CallStatus.Fail)
            {
                if (ex != null)
                    throw new Exception("Fail call function", ex);
                else
                    throw new Exception("Fail call function");
            }
        }

        public static void ThrowIfFail(CallResult<Exception> result)
        {
            if (result.Status == CallStatus.Fail)
            {
                throw new Exception("Fail call function", result.Data);
            }
        }

        public static CallResult<T> ThrowIfFail<T>(CallResult<T> result)
        {
            if (result.Status == CallStatus.Fail)
            {
                throw new Exception(result.Message);
            }
            return result;
        }
    }
}
