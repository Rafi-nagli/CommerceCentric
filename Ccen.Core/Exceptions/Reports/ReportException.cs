using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Exceptions.Reports
{
    public class ReportException : Exception
    {
        public ReportException(string message) : base(message)
        {
            
        }
    }
}
