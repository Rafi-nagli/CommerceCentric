using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models.Calls
{
    public class WithMessages<T>
    {
        public T Value { get; set; }
        public IList<MessageString> Messages { get; set; }

        public bool IsSucess
        {
            get { return Messages == null || Messages.All(m => m.IsSuccess || m.IsInfo); }
        }

        public bool IsError
        {
            get { return Messages == null || Messages.All(m => m.IsError || m.IsWarning); }
        }

        public WithMessages(T value)
        {
            Value = value;
        }

        public WithMessages(T value, IList<MessageString> messages)
        {
            Value = value;
            Messages = messages;
        }
    }
}
