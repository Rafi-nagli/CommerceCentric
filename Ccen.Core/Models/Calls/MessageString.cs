using System.Collections.Generic;
using System.Linq;

namespace Amazon.Core.Models.Calls
{
    public static class MessageStringExtensions
    {
        public static IList<MessageString> AddRangeExcludeDuplicates(this IList<MessageString> messages, IList<MessageString> newMessages)
        {
            foreach (var message in newMessages)
            {
                if (!messages.Any(m => m.Message == message.Message
                    && m.Status == message.Status))
                {
                    messages.Add(message);
                }
            }
            return messages;
        }
    }



    public class MessageString
    {
        public string Key { get; set; }
        public string Message { get; set; }
        public MessageStatus Status { get; set; }

        public bool IsSuccess
        {
            get { return Status == MessageStatus.Success; }
        }

        public bool IsWarning
        {
            get { return Status == MessageStatus.Warning; }
        }

        public bool IsError
        {
            get { return Status == MessageStatus.Error; }
        }

        public bool IsInfo
        {
            get { return Status == MessageStatus.Info; }
        }

        public static MessageString From(string key, string message)
        {
            return new MessageString()
            {
                Key = key,
                Message = message
            };
        }

        public static MessageString From(Message message)
        {
            MessageStatus status = MessageStatus.Info;
            if (message.Type == MessageTypes.Error)
                status = MessageStatus.Error;
            if (message.Type == MessageTypes.Warning)
                status = MessageStatus.Warning;
            if (message.Type == MessageTypes.Fatal)
                status = MessageStatus.Error;
            if (message.Type == MessageTypes.Info)
                status = MessageStatus.Info;

            return new MessageString()
            {
                Message = message.Text,
                Status = status
            };
        }

        public static MessageString Success(string message)
        {
            return new MessageString()
            {
                Message = message,
                Status = MessageStatus.Success
            };
        }

        public static MessageString Warning(string message)
        {
            return new MessageString()
            {
                Message = message,
                Status = MessageStatus.Warning
            };
        }

        public static MessageString Info(string message)
        {
            return new MessageString()
            {
                Message = message,
                Status = MessageStatus.Info
            };
        }

        public static MessageString Error(string message)
        {
            return new MessageString()
            {
                Message = message,
                Status = MessageStatus.Error
            };
        }

        public static MessageString Error(string key, string message)
        {
            return new MessageString()
            {
                Key = key,
                Message = message,
                Status = MessageStatus.Error
            };
        }

        public override string ToString()
        {
            return "m=" + Message + ", status=" + Status;
        }

        public static string ToString(IList<MessageString> messages)
        {
            var result = "";
            if (messages == null)
                return result;
            foreach (var message in messages)
                result += message.ToString();

            return result;
        }
    }
}