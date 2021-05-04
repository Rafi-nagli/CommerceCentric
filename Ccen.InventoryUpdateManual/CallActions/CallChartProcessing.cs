using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Common.Helpers;
using Amazon.Common.Models;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities.Inventory;
using Amazon.Core.Models;
using Amazon.Model.Implementation;
using Amazon.Model.Implementation.Charts;
using Amazon.Model.Implementation.Markets;

namespace Amazon.InventoryUpdateManual.CallActions
{
    public class CallChartProcessing
    {
        private IDbFactory _dbFactory;
        private ITime _time;
        private ILogService _log;

        public CallChartProcessing(IDbFactory dbFactory,
            ITime time,
            ILogService log)
        {
            _dbFactory = dbFactory;
            _time = time;
            _log = log;
        }

        public void AddListingErrorsPoints()
        {
            var maker = new ListingErrorChartMaker(_dbFactory, _time, _log);
            maker.AddPoints();
        }
    }
}
