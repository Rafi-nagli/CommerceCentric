using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Exceptions
{
    public class CleanseAddressException : Exception
    {
        public CleanseAddressException(string message) : base(message) { }
    }
}
