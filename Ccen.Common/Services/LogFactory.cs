using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core.Contracts;
using log4net;

namespace Amazon.Common.Services
{
    public class LogFactory
    {
        public static ILogService From(ILog log, string userId = "")
        {
            return new FileLogService(log, userId);
        }

        public static ILogService From(string tag)
        {
            return new FileLogService(LogManager.GetLogger(tag));
        }

        public static ILogService Default
        {
            get
            {
                var log = LogManager.GetLogger("Default");
                return new FileLogService(log);
            }
        }

        public static ILogService Console
        {
            get
            {
                var log = LogManager.GetLogger("Default");
                return new FileLogService(log, true);
            }
        }

        public static ILogService DB
        {
            get
            {
                var log = LogManager.GetLogger("DBLogger");
                return new FileLogService(log, true);
            }
        }

        public static ILogService Empty
        {
            get
            {
                return new EmptyLogService();
            }
        }

    }
}
