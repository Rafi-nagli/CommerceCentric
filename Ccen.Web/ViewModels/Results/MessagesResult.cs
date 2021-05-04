using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Amazon.Core.Models.Calls;
using Amazon.Web.ViewModels.Results;

namespace Amazon.Web.ViewModels.Messages
{
    public class MessagesResult
    {
        public bool IsSuccess { get; set; }
        public IList<MessageString> Messages { get; set; }

        public MessagesResult()
        {
            
        }

        public MessagesResult(bool isSuccess)
        {
            IsSuccess = isSuccess;
            Messages = new List<MessageString>();
        }

        public MessagesResult Success(string message)
        {
            Messages.Add(MessageString.Success(message));
            return this;
        }

        public MessagesResult Error(string message)
        {
            Messages.Add(MessageString.Error(message));
            return this;
        }

        public MessagesResult Warning(string message)
        {
            Messages.Add(MessageString.Warning(message));
            return this;
        }

        public MessagesResult Info(string message)
        {
            Messages.Add(MessageString.Info(message));
            return this;
        }
    }
}