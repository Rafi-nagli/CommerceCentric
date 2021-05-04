using System;

namespace Amazon.Core.Exceptions
{
    public class StampsException : Exception
    {
        public StampsException(string message) : base("Communication with stamps: " + message)
        {
            
        }

        public StampsException(string message, Exception ex) : base("Communication with stamps: " + message, ex)
        {
            
        }
    }
}
