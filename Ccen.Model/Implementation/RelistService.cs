using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;

namespace Amazon.Model.Implementation
{
    public class RelistService
    {
        private IDbFactory _dbFactory;
        private ICacheService _cache;
        private ILogService _log;
        private ITime _time;

        public RelistService(IDbFactory dbFactory,
            ICacheService cache,
            ILogService log,
            ITime time)
        {
            _dbFactory = dbFactory;
            _cache = cache;
        }

    }
}
