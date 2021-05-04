using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.Core.Models.SystemActions.SystemActionDatas;
using Amazon.DTO;
using Jet.Api;
using Newtonsoft.Json;
using Walmart.Api;

namespace Amazon.Model.Implementation.Markets.Walmart
{
    public class JetOrderCancellation
    {
        private JetApi _api;
        private ILogService _log;
        private ITime _time;
        private ISystemActionService _actionService;

        public JetOrderCancellation(JetApi api, 
            ISystemActionService actionService,
            ILogService log, 
            ITime time)
        {
            _api = api;
            _log = log;
            _time = time;
            _actionService = actionService;
        }

        public void ProcessCancellations(IUnitOfWork db)
        {
            //NOTHING
        }
    }
}
