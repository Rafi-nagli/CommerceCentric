using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core.Contracts;

namespace Amazon.Common.Services
{
    public class FileLogEntity : ILogEntry
    {
        private ILogService _logService;

        private Guid _entryId;
        public Guid EntryId { get { return _entryId; } }
        
        public FileLogEntity(ILogService logService, Guid entryId)
        {
            _logService = logService;
            _entryId = entryId;
        }
        
        public ILogEntry Debug(string message)
        {
            return _logService.Debug(message, this);
        }

        public ILogEntry Debug(string message, params string[] values)
        {
            return _logService.Debug(message, this, values);
        }

        public ILogEntry Debug(string message, Exception ex)
        {
            return _logService.Debug(message, ex, this);
        }


        public ILogEntry Info(string message)
        {
            return _logService.Info(message, this);
        }

        public ILogEntry Info(string message, params string[] values)
        {
            return _logService.Info(message, this, values);
        }

        public ILogEntry Info(string message, Exception ex)
        {
            return _logService.Info(message, ex, this);
        }


        public ILogEntry Warn(string message)
        {
            return _logService.Warn(message, this);
        }

        public ILogEntry Warn(string message, params string[] values)
        {
            return _logService.Warn(message, this, values);
        }

        public ILogEntry Warn(string message, Exception ex)
        {
            return _logService.Warn(message, ex, this);
        }


        public ILogEntry Error(string message)
        {
            return _logService.Error(message, this);
        }

        public ILogEntry Error(string message, params string[] values)
        {
            return _logService.Error(message, this, values);
        }

        public ILogEntry Error(string message, Exception ex)
        {
            return _logService.Error(message, ex, this);
        }


        public ILogEntry Fatal(string message)
        {
            return _logService.Fatal(message, this);
        }

        public ILogEntry Fatal(string message, params string[] values)
        {
            return _logService.Fatal(message, this, values);
        }

        public ILogEntry Fatal(string message, Exception ex)
        {
            return _logService.Fatal(message, ex, this);
        }
    }
}
