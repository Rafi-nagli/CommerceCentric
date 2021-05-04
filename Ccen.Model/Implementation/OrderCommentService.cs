using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities;
using Amazon.Core.Models;
using Amazon.Core.Models.SystemActions.SystemActionDatas;
using Amazon.Model.Models;

namespace Amazon.Model.Implementation
{
    public class OrderCommentService
    {
        private IDbFactory _dbFactory;
        private ILogService _log;
        private ITime _time;
        private ISystemActionService _actionService;

        public OrderCommentService(IDbFactory dbFactory,
            ILogService log,
            ITime time,
            ISystemActionService actionService)
        {
            _dbFactory = dbFactory;
            _log = log;
            _time = time;
            _actionService = actionService;
        }

        public void ProcessSystemAction()
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var commentActions = _actionService.GetUnprocessedByType(db, SystemActionType.AddComment, null, null);

                foreach (var action in commentActions)
                {
                    var actionStatus = SystemActionStatus.None;
                    try
                    {
                        var inputData = SystemActionHelper.FromStr<AddCommentInput>(action.InputData);
                        var orderId = inputData.OrderId;
                        if (!orderId.HasValue && !String.IsNullOrEmpty(inputData.OrderNumber))
                        {
                            var order = db.Orders.GetByCustomerOrderNumber(inputData.OrderNumber);
                            if (order == null)
                            {
                                throw new ArgumentException("Can't find order", "OrderNumber");
                            }

                            orderId = order.Id;
                        }

                        if (orderId.HasValue)
                        {
                            db.OrderComments.Add(new OrderComment()
                            {
                                OrderId = orderId.Value,
                                Message = inputData.Message,
                                Type = inputData.Type,
                                LinkedEmailId = inputData.LinkedEmailId,

                                CreateDate = _time.GetAppNowTime(),
                                CreatedBy = inputData.By,
                            });
                            db.Commit();

                            actionStatus = SystemActionStatus.Done;
                            _log.Info("Comment was added, actionId=" + action.Id);
                        }
                        else
                        {
                            actionStatus = SystemActionStatus.NotFoundEntity;
                            _log.Info("Can't find order, orderId=" + inputData.OrderId + ", orderNumber=" + inputData.OrderNumber);
                        }
                        
                    }
                    catch (Exception ex)
                    {
                        actionStatus = SystemActionStatus.Fail;
                        if (action.CreateDate > _time.GetUtcTime().AddHours(-6))
                            actionStatus = SystemActionStatus.None;

                        _log.Error("Fail add comment action, actionId=" + action.Id + ", status=" + actionStatus, ex);
                    }

                    _actionService.SetResult(db, 
                        action.Id,
                        actionStatus,
                        null,
                        null);
                }

                db.Commit();
            }
        }
    }
}
