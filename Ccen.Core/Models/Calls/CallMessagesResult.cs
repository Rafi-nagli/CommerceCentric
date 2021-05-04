using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models.Calls
{
    public class CallMessagesResult<T>
    {
        public CallStatus Status { get; set; }
        public IList<MessageString> Messages { get; set; }
        public Exception Exception { get; set; }
        public T Data { get; set; }

        public bool IsSuccess
        {
            get { return Status == CallStatus.Success; }
        }

        public static CallMessagesResult<T> Success(T data)
        {
            return new CallMessagesResult<T>()
            {
                Data = data,
                Status = CallStatus.Success
            };
        }

        public static CallMessagesResult<T> Fail(MessageString message, Exception ex)
        {
            return new CallMessagesResult<T>()
            {
                Messages = new List<MessageString> { message },
                Exception = ex,
                Status = CallStatus.Fail
            };
        }
    }
}
