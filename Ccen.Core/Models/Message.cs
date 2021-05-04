using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models
{
    public class Message
    {
        public string Text { get; set; }
        public MessageTypes Type { get; set; }

        public Message()
        {
            
        }

        public Message(string text, MessageTypes type)
        {
            Text = text;
            Type = type;
        }
    }
}
