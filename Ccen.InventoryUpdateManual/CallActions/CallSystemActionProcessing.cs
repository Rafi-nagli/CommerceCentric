using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities;
using Amazon.Core.Models;
using Amazon.Core.Models.SystemActions.SystemActionDatas;
using Amazon.Model.Implementation;
using log4net;
using Newtonsoft.Json;

namespace Amazon.InventoryUpdateManual.CallActions
{
    public class CallSystemActionProcessing
    {
        private ILogService _log;
        private IDbFactory _dbFactory;
        private ITime _time;
        private ISystemActionService _actionService;

        public CallSystemActionProcessing(ILogService log,
            IDbFactory dbFactory,
            ITime time,
            ISystemActionService actionService)
        {
            _log = log;
            _dbFactory = dbFactory;
            _time = time;
            _actionService = actionService;
        }

        public void ProcessRefundsAddComments()
        {
            var orderNumbers = new string[]
            {
                "002-4984322-1582658",
                "115-7133068-0543442",
                "107-3678266-6682660",
                "104-9680685-2993855",
                "114-2903306-3741829",
                "110-9470669-5336239",
                "701-4988323-0546632",
                "108-4618630-0086635",
                "109-8552981-9073035",
                "114-6351653-1882624",
                "111-9932301-2780224",
                "109-4832961-2930620",
                "108-4433058-4988231",
                "701-2615147-4537063",
                "106-3092827-4661027",
                "102-6937670-6201809",
                "702-0464553-4177858"
            };

            using (var db = _dbFactory.GetRWDb())
            {
                var actions = db.SystemActions.GetAll()
                        .Where(a => a.Type == (int) SystemActionType.UpdateOnMarketReturnOrder)
                        .ToList();

                foreach (var action in actions)
                {
                    if (!orderNumbers.Contains(action.Tag))
                        continue;

                    var data = JsonConvert.DeserializeObject<ReturnOrderInput>(action.InputData);
                    var order = db.Orders.GetByOrderNumber(data.OrderNumber);
                    var unsuitableData = false;
                    var unsuitableMessage = "";

                    var message = String.Format(
                            "System accidently deducted shipping twice from a refund. If client complains, confirm additional refund wasn’t processed yet, and refund ONCE additional {0}{1}",
                            PriceHelper.FormatCurrency(order.TotalPriceCurrency),
                            data.Items.Sum(i => i.DeductShippingPrice));

                    db.OrderComments.Add(new OrderComment()
                    {
                        OrderId = order.Id,
                        Message = message,
                        Type = (int)CommentType.ReturnExchange,
                        CreateDate = _time.GetAppNowTime(),
                    });

                    db.Commit();
                }
            }
        }

        public void ProcessAddCommentSystemActions()
        {
            var commentService = new OrderCommentService(_dbFactory,
                _log,
                _time,
                _actionService);

            commentService.ProcessSystemAction();
        }
    }
}
