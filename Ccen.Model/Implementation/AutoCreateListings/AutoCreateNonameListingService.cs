using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Api.Exports;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Contracts.Markets;
using Amazon.Core.Entities;
using Amazon.Core.Helpers;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.Core.Models.SystemActions;
using Amazon.DTO;
using Amazon.DTO.Inventory;
using Walmart.Api;

namespace Amazon.Model.Implementation
{
    public class AutoCreateNonameListingService : AutoCreateBaseListingService
    {
        private ILogService _log;
        private ITime _time;
        private IDbFactory _dbFactory;

        public AutoCreateNonameListingService(ILogService log,
            ITime time,
            IDbFactory dbFactory,
            ICacheService cacheService,
            IBarcodeService barcodeService,
            IEmailService emailService,
            IItemHistoryService itemHistoryService,
            bool isDebug) : base(log, time, dbFactory, cacheService, barcodeService, emailService, itemHistoryService, isDebug)
        {
            _log = log;
            _time = time;
            _dbFactory = dbFactory;
        }


        public override void CreateListings()
        {
            throw new NotSupportedException();
        }
    }
}
