using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Amazon.Core.Contracts
{
    public interface ILogService : ILogEntry
    {
        ILogEntry Debug(string message, ILogEntry parent);
        ILogEntry Debug(string message, ILogEntry parent, params string[] values);
        ILogEntry Debug(string message, Exception ex, ILogEntry parent);

        ILogEntry Info(string message, ILogEntry parent);
        ILogEntry Info(string message, ILogEntry parent, params string[] values);
        ILogEntry Info(string message, Exception ex, ILogEntry parent);

        ILogEntry Warn(string message, ILogEntry parent);
        ILogEntry Warn(string message, ILogEntry parent, params string[] values);
        ILogEntry Warn(string message, Exception ex, ILogEntry parent);

        ILogEntry Error(string message, ILogEntry parent);
        ILogEntry Error(string message, ILogEntry parent, params string[] values);
        ILogEntry Error(string message, Exception ex, ILogEntry parent);

        ILogEntry Fatal(string message, ILogEntry parent);
        ILogEntry Fatal(string message, ILogEntry parent, params string[] values);
        ILogEntry Fatal(string message, Exception ex, ILogEntry parent);
    }
}
