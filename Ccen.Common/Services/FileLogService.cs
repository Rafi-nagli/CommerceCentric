using System;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using log4net;

namespace Amazon.Common.Services
{
    public class FileLogService : ILogService
    {
        private ILog _log = null;
        private string _userId = String.Empty;
        private bool _writeToConsole = false;

        private bool _hierarchical = false;

        public Guid EntryId
        {
            get { return Guid.Empty; }
        }


        public FileLogService(ILog log, string userId = "")
        {
            if (log == null)
                throw new ArgumentNullException("log");

            _userId = userId;
            _log = log;
        }

        public FileLogService(ILog log, bool writeToConsole)
        {
            if (log == null)
                throw new ArgumentNullException("log");

            _writeToConsole = writeToConsole;
            _log = log;
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


        private void LogToConsole(string message, Exception ex, ConsoleColor color)
        {
            var current = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + " - " + message);
            if (ex != null)
            {
                Console.WriteLine(ex.Message);
            }
            Console.ForegroundColor = current;
        }

        public ILogEntry Log(LogMessageType messageType,
            string message,
            Exception ex,
            ILogEntry parent,
            params string[] values)
        {
            var entryId = Guid.NewGuid();
            var msg = String.Empty;

            if (_hierarchical)
                msg = " - @ - " + entryId + " - " + (parent != null ? parent.EntryId.ToString() : "") + " - " + _userId + Environment.NewLine;
            msg += message;

            if (values != null)
            {
                for (int i = 0; i < values.Length; i++)
                    msg += Environment.NewLine + "value" + i + "=" + values[i];
            }

            switch (messageType)
            {
                case LogMessageType.Debug:
                    _log.Debug(msg, ex);
                    break;
                case LogMessageType.Info:
                    _log.Info(msg, ex);
                    break;
                case LogMessageType.Warning:
                    _log.Warn(msg, ex);
                    break;
                case LogMessageType.Error:
                    _log.Error(msg, ex);
                    break;
                case LogMessageType.Fatal:
                    _log.Fatal(msg, ex);
                    break;
            }

            if (_writeToConsole)
            {
                switch (messageType)
                {
                    case LogMessageType.Error:
                    case LogMessageType.Fatal:
                        LogToConsole(msg, ex, ConsoleColor.Red);
                        break;
                    case LogMessageType.Warning:
                        LogToConsole(msg, ex, ConsoleColor.Magenta);
                        break;
                    default:
                        LogToConsole(msg, ex, ConsoleColor.White);
                        break;
                }
            }

            return new FileLogEntity(this, entryId);
        }
    }
}
