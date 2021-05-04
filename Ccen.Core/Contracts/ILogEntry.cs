using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Contracts
{
    public interface ILogEntry
    {
        Guid EntryId { get; }

        ILogEntry Debug(string message);
        ILogEntry Debug(string message, params string[] values);
        ILogEntry Debug(string message, Exception ex);

        ILogEntry Info(string message);
        ILogEntry Info(string message, params string[] values);
        ILogEntry Info(string message, Exception ex);
        
        ILogEntry Warn(string message);
        ILogEntry Warn(string message, params string[] values);
        ILogEntry Warn(string message, Exception ex);

        ILogEntry Error(string message);
        ILogEntry Error(string message, params string[] values);
        ILogEntry Error(string message, Exception ex);

        ILogEntry Fatal(string message);
        ILogEntry Fatal(string message, params string[] values);
        ILogEntry Fatal(string message, Exception ex);
    }
}
