using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models.Calls
{
    public class CallMessagesResultVoid
    {
        public CallStatus Status { get; set; }
        public IList<MessageString> Messages { get; set; }
        public Exception Exception { get; set; }

        public bool IsSuccess
        {
            get { return Status == CallStatus.Success; }
        }

        public void Combine(CallMessagesResultVoid result)
        {
            if (Status < result.Status)
                Status = result.Status;

            if (result.Messages != null)
            {
                if (Messages != null)
                    Messages.AddRangeExcludeDuplicates(result.Messages);
                else
                    Messages = result.Messages;
            }
            if (Exception == null)
                Exception = result.Exception;
        }

        public static CallMessagesResultVoid Success()
        {
            return new CallMessagesResultVoid()
            {
                Status = CallStatus.Success
            };
        }

        public static CallMessagesResultVoid Fail(MessageString message, Exception ex)
        {
            return new CallMessagesResultVoid()
            {
                Messages = new List<MessageString> { message },
                Exception = ex,
                Status = CallStatus.Fail
            };
        }

        public static CallMessagesResultVoid Fail(string message, Exception ex)
        {
            return new CallMessagesResultVoid()
            {
                Messages = new List<MessageString> { MessageString.Error(message) },
                Exception = ex,
                Status = CallStatus.Fail
            };
        }

        public static CallMessagesResultVoid GetResultBySingleMessage(string mess, MessageStatus messageStatus, CallStatus callStatus)
        {
            return new CallMessagesResultVoid()
            {
                Messages = new List<MessageString>() { new MessageString() { Message = mess, Status = messageStatus } },
                Status = callStatus
            };
        }
    }
}
