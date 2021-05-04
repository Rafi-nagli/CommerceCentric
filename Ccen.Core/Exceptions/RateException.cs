using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Exceptions
{
    public class RateException : Exception
    {
        public RateException(string message): base(message)
        {

        }

        public RateException(string message, Exception ex): base(message, ex)
        {

        }
    }
}
