using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities.Orders;
using Amazon.Core.Models;
using Amazon.Core.Models.SystemActions.SystemActionDatas;
using Amazon.DTO;
using Amazon.DTO.Orders;
using Newtonsoft.Json;
using Walmart.Api;

namespace Amazon.Model.Implementation.Markets.Walmart
{
    public class WalmartReturnInfoReader
    {
        private WalmartApi _api;
        private ILogService _log;
        private ITime _time;
        private IDbFactory _dbFactory;
        private string _reportBaseDirectory;
        private string _feedBaseDirectory;

        public WalmartReturnInfoReader(ILogService log, 
            ITime time,
            WalmartApi api, 
            IDbFactory dbFactory,
            string reportBaseDirectory,
            string feedBaseDirectory)
        {
            _api = api;
            _log = log;
            _time = time;
            _dbFactory = dbFactory;
            _reportBaseDirectory = reportBaseDirectory;
            _feedBaseDirectory = feedBaseDirectory;
        }

        public void UpdateReturnInfo()
        {
            var sourceItems = _api.GetReturns();
            ProcessReturns(_api, _time, sourceItems);
        }

        private void ProcessReturns(IMarketApi api,
            ITime time,
            IList<ReturnRequestDTO> sourceItems)
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var returnItems = sourceItems.Select(i => (ReturnRequestDTO)i).ToList();

                var updatedList = new List<string>(returnItems.Count);
                foreach (var item in returnItems)
                {
                    var returnRequest = db.ReturnRequests.GetAll()
                        .OrderByDescending(r => r.CreateDate)
                        .FirstOrDefault(r => r.OrderNumber == item.OrderNumber
                            && r.MarketReturnId == item.MarketReturnId);

                    if (returnRequest == null)
                    {
                        _log.Info("Return request has been added, orderNumber=" + item.OrderNumber + ", rma=" + item.MarketReturnId);

                        returnRequest = new ReturnRequest()
                        {
                            Type = item.Type,
                            MarketReturnId = item.MarketReturnId,
                            ReturnByDate = item.ReturnByDate,
                            Status = item.Status,
                            StatusDate = item.StatusDate,
                            RequestedRefundAmount = item.RequestedRefundAmount,

                            Reason = item.Reason,
                            ItemName = item.ItemName,
                            Details = item.Details,
                            SKU = item.SKU,
                            OrderNumber = item.OrderNumber,
                            ReceiveDate = item.ReceiveDate,
                            Quantity = item.Quantity,

                            CreateDate = _time.GetAppNowTime()
                        };
                        db.ReturnRequests.Add(returnRequest);
                        db.Commit();
                    }
                    else
                    {
                        returnRequest.Type = item.Type;
                        returnRequest.MarketReturnId = item.MarketReturnId;
                        returnRequest.ReturnByDate = item.ReturnByDate;
                        returnRequest.Status = item.Status;
                        returnRequest.StatusDate = item.StatusDate;
                        returnRequest.RequestedRefundAmount = item.RequestedRefundAmount;

                        returnRequest.Reason = item.Reason;
                        returnRequest.ItemName = item.ItemName;
                        returnRequest.Details = item.Details;
                        returnRequest.SKU = item.SKU;
                        returnRequest.OrderNumber = item.OrderNumber;
                        returnRequest.ReceiveDate = item.ReceiveDate;
                        returnRequest.Quantity = item.Quantity;
                    }

                    var dbReturnRequestItems = db.ReturnRequestItems
                        .GetAll()
                        .Where(r => r.ReturnRequestId == returnRequest.Id)
                        .ToList();

                    var dbOrder = db.Orders.GetAllByCustomerOrderNumber(returnRequest.OrderNumber).FirstOrDefault();
                    var dbOrderItems = dbOrder != null ? db.OrderItems.GetAll().Where(oi => oi.OrderId == dbOrder.Id).ToList() : new List<OrderItem>();

                    foreach (var subItem in item.Items)
                    {
                        var dbOrderItem = dbOrderItems.FirstOrDefault(i => i.ItemOrderIdentifier == subItem.LineItemId + "_" + subItem.LineNumber);

                        var existDbItem = dbReturnRequestItems.FirstOrDefault(i => i.LineItemId == subItem.LineItemId);
                        if (existDbItem == null)
                        {
                            _log.Info("Add return request item: " + subItem.SKU);
                            db.ReturnRequestItems.Add(new ReturnRequestItem()
                            {
                                ReturnRequestId = returnRequest.Id,
                                LineNumber = subItem.LineNumber,
                                LineItemId = subItem.LineItemId,
                                ItemName = subItem.ItemName,
                                SKU = subItem.SKU,
                                Quantity = subItem.Quantity,
                                RefundTotalAmount = subItem.RefundTotalAmount,
                                CreateDate = _time.GetAppNowTime(),
                                StyleId = dbOrderItem?.StyleId,
                                StyleItemId = dbOrderItem?.StyleItemId,
                            });
                        }
                        else
                        {
                            existDbItem.RefundTotalAmount = subItem.RefundTotalAmount;
                            existDbItem.StyleId = dbOrderItem?.StyleId;
                            existDbItem.StyleItemId = dbOrderItem?.StyleItemId;

                        }
                    }

                    db.Commit();
                }
            }
        }
    }
}
