using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Core.Models.Calls;
using Amazon.Web.ViewModels.Messages;

namespace Amazon.Web.ViewModels.Results
{
    public class ValueMessageResult<T> : ValueResult<T>
    {
        public IList<MessageString> Messages { get; set; }

        public ValueMessageResult()
        {
            
        }

        public ValueMessageResult(bool isSuccess, IList<MessageString> messages, T data)
        {
            IsSuccess = isSuccess;
            Messages = messages;
            Data = data;
        }

        public static ValueMessageResult<T> Success(IList<MessageString> messages, T data = default(T))
        {
            return new ValueMessageResult<T>(true, messages, data);
        }

        public static ValueMessageResult<T> Error(IList<MessageString> messages, T data = default(T))
        {
            return new ValueMessageResult<T>(false, messages, data);
        }
    }
}