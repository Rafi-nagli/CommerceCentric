using System;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using log4net;

namespace Amazon.Common.Services
{
    public class EmptyLogService : ILogService
    {
        private bool _writeToConsole = false;

        private bool _hierarchical = false;

        public Guid EntryId
        {
            get { return Guid.Empty; }
        }


        public EmptyLogService()
        {
        }
        
        public ILogEntry Debug(string message)
        {
            return Log(LogMessageType.Debug, message, null, null);
        }

        public ILogEntry Debug(string message, params string[] values)
        {
            return Log(LogMessageType.Debug, message, null, null, values);
        }

        public ILogEntry Debug(string message, Exception ex)
        {
            return Log(LogMessageType.Debug, message, ex, null);
        }

        public ILogEntry Debug(string message, ILogEntry parent)
        {
            return Log(LogMessageType.Debug, message, null, parent);
        }

        public ILogEntry Debug(string message, ILogEntry parent, params string[] values)
        {
            return Log(LogMessageType.Debug, message, null, parent, values);
        }

        public ILogEntry Debug(string message, Exception ex, ILogEntry parent)
        {
            return Log(LogMessageType.Debug, message, ex, parent);
        }



        public ILogEntry Info(string message)
        {
            return Log(LogMessageType.Info, message, null, null);
        }

        public ILogEntry Info(string message, params string[] values)
        {
            return Log(LogMessageType.Info, message, null, null, values);
        }

        public ILogEntry Info(string message, Exception ex)
        {
            return Log(LogMessageType.Info, message, ex, null);
        }

        public ILogEntry Info(string message, ILogEntry parent)
        {
            return Log(LogMessageType.Info, message, null, parent);
        }

        public ILogEntry Info(string message, ILogEntry parent, params string[] values)
        {
            return Log(LogMessageType.Info, message, null, parent, values);
        }

        public ILogEntry Info(string message, Exception ex, ILogEntry parent)
        {
            return Log(LogMessageType.Info, message, ex, parent);
        }



        public ILogEntry Warn(string message)
        {
            return Log(LogMessageType.Warning, message, null, null);
        }

        public ILogEntry Warn(string message, params string[] values)
        {
            return Log(LogMessageType.Warning, message, null, null, values);
        }

        public ILogEntry Warn(string message, Exception ex)
        {
            return Log(LogMessageType.Warning, message, ex, null);
        }

        public ILogEntry Warn(string message, ILogEntry parent)
        {
            return Log(LogMessageType.Warning, message, null, parent);
        }

        public ILogEntry Warn(string message, ILogEntry parent, params string[] values)
        {
            return Log(LogMessageType.Warning, message, null, parent, values);
        }

        public ILogEntry Warn(string message, Exception ex, ILogEntry parent)
        {
            return Log(LogMessageType.Warning, message, ex, parent);
        }


        public ILogEntry Error(string message)
        {
            return Log(LogMessageType.Error, message, null, null);
        }

        public ILogEntry Error(string message, params string[] values)
        {
            return Log(LogMessageType.Error, message, null, null, values);
        }

        public ILogEntry Error(string message, Exception ex)
        {
            return Log(LogMessageType.Error, message, ex, null);
        }

        public ILogEntry Error(string message, ILogEntry parent)
        {
            return Log(LogMessageType.Error, message, null, parent);
        }

        public ILogEntry Error(string message, ILogEntry parent, params string[] values)
        {
            return Log(LogMessageType.Error, message, null, parent, values);
        }

        public ILogEntry Error(string message, Exception ex, ILogEntry parent)
        {
            return Log(LogMessageType.Error, message, ex, parent, null);
        }


        public ILogEntry Fatal(string message)
        {
            return Log(LogMessageType.Fatal, message, null, null);
        }

        public ILogEntry Fatal(string message, params string[] values)
        {
            return Log(LogMessageType.Fatal, message, null, null, values);
        }

        public ILogEntry Fatal(string message, Exception ex)
        {
            return Log(LogMessageType.Fatal, message, ex, null);
        }

        public ILogEntry Fatal(string message, ILogEntry parent)
        {
            return Log(LogMessageType.Fatal, message, null, parent);
        }

        public ILogEntry Fatal(string message, ILogEntry parent, params string[] values)
        {
            return Log(LogMessageType.Fatal, message, null, parent, values);
        }

        public ILogEntry Fatal(string message, Exception ex, ILogEntry parent)
        {
            return Log(LogMessageType.Fatal, message, ex, parent, null);
        }

        public ILogEntry Log(LogMessageType messageType,
            string message,
            Exception ex,
            ILogEntry parent,
            params string[] values)
        {
            var entryId = Guid.NewGuid();
            
            return new FileLogEntity(this, entryId);
        }
    }
}
