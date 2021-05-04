using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Amazon.Web.ViewModels.Messages
{
    public class MessageResult : ValueResult<string>
    {
        public MessageResult() { }

        public MessageResult(bool isSuccess, string message, string data)
        {
            IsSuccess = isSuccess;
            Message = message;
            Data = data;
        }

        public static MessageResult Success(string message, string data = null)
        {
            return new MessageResult(true, message, data);
        }

        public static MessageResult Success()
        {
            return new MessageResult(true, String.Empty, String.Empty);
        }

        public static MessageResult Error(string message, string data = null)
        {
            return new MessageResult(false, message, data);
        }

        public static MessageResult Error()
        {
            return new MessageResult(false, String.Empty, String.Empty);
        }
    }
}