using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Contracts.Orders;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models;
using Amazon.Core.Models.SystemActions;
using Amazon.Core.Models.SystemActions.SystemActionDatas;
using Amazon.DTO;
using Amazon.DTO.Users;
using Amazon.Model.General;
using Amazon.Model.Implementation.Errors;
using Amazon.Model.Implementation.Sorting;
using Newtonsoft.Json;

namespace Amazon.Model.Implementation.Rates
{
    public class LabelBatchService : ILabelBatchService
    {
        private ILabelService _labelService;
        private IDbFactory _dbFactory;
        private ISystemActionService _actionService;
        private ILogService _log;
        private ITime _time;
        private IWeightService _weightService;
        private IServiceFactory _serviceFactory;
        private IEmailService _emailService;
        private IBatchManager _batchManager;
        private IFileMaker _pdfMaker;
        private IAddressService _addressService;
        private IOrderHistoryService _orderHistoryService;
        private string _defaultCustomType;
        private string _labelDirectory;
        private string _reserveDirectory;
        private string _templateDirectory;
        private bool _isSampleMode;
        private Config _config;

        public class Config
        {
            public string[] PrintErrorsToEmails { get; set; }
            public string[] PrintErrorsCCEmails { get; set; }
        }

        public LabelBatchService(IDbFactory dbFactory,
            ISystemActionService actionService,
            ILogService log,
            ITime time,
            IWeightService weightService,
            IServiceFactory serviceFactory,
            IEmailService emailService,
            IBatchManager batchManager,
            IFileMaker pdfMaker,
            IAddressService addressService,
            IOrderHistoryService orderHistoryService,
            string defaultCustomType,
            string labelDirectory,
            string reserveDirectory,
            string templateDirectory,
            Config config,
            bool isSampleMode)
        {
            _dbFactory = dbFactory;
            _actionService = actionService;
            _log = log;
            _time = time;
            _weightService = weightService;
            _serviceFactory = serviceFactory;
            _emailService = emailService;
            _batchManager = batchManager;
            _pdfMaker = pdfMaker;
            _addressService = addressService;
            _orderHistoryService = orderHistoryService;

            _defaultCustomType = defaultCustomType;
            _labelDirectory = labelDirectory;
            _reserveDirectory = reserveDirectory;
            _templateDirectory = templateDirectory;
            _config = config;
            _isSampleMode = isSampleMode;
        }

        public PrintLabelResult PrintLabel(long orderId, long companyId, long? by)
        {
            var printLabelResult = new PrintLabelResult()
            {
                IsPrintStarted = false
            };

            var syncInfo = new DbSyncInformer(_dbFactory,
                    _log,
                    _time,
                    SyncType.PostagePurchase,
                    "",
                    MarketType.None,
                    String.Empty);

            using (var db = _dbFactory.GetRWDb())
            {
                var company = db.Companies.GetByIdWithSettingsAsDto(companyId);
                var companyAddress = new CompanyAddressService(company);

                var shipmentProviders = _serviceFactory.GetShipmentProviders(_log,
                    _time,
                    _dbFactory,
                    _weightService,
                    company.ShipmentProviderInfoList,
                    _defaultCustomType,
                    _labelDirectory,
                    _reserveDirectory,
                    _templateDirectory);

                var labelService = new LabelService(shipmentProviders, _log, _time, _dbFactory, _emailService, _pdfMaker, _addressService);

                CompanyHelper.UpdateBalance(db, company, shipmentProviders, true, _time.GetAppNowTime());

                var shippingList =
                    db.OrderShippingInfos.GetOrderInfoWithItems(_weightService,
                        new[] {orderId}.ToList(), 
                        SortMode.ByLocation,
                        unmaskReferenceStyle: false,
                        includeSourceItems: false).ToList();
                shippingList = shippingList.Where(sh => !sh.OnHold).ToList();

                //NOTE: Update from address
                var dropShipperList = db.DropShippers.GetAllAsDto().ToList();
                foreach (var shipping in shippingList)
                {
                    shipping.ReturnAddress = dropShipperList.FirstOrDefault(ds => ds.Id == shipping.DropShipperId)?.GetAddressDto();
                }


                printLabelResult = labelService.PrintLabels(db,
                    company,
                    companyAddress,
                    syncInfo,
                    null,
                    shippingList,
                    true,
                    null,
                    null,
                    null,
                    _labelDirectory,
                    _isSampleMode,
                    _time.GetAppNowTime(),
                    by);

                //Apply new balance to Session
                CompanyHelper.UpdateBalance(db, company, shipmentProviders, false, _time.GetAppNowTime());

                if (printLabelResult.IsPrintStarted)
                {
                    SaveBatchPrintResultToDb(db,
                        _log,
                        null,
                        printLabelResult,
                        shippingList,
                        _time.GetAppNowTime(),
                        by);

                    if (printLabelResult.FailedIds.Any())
                    {
                        printLabelResult.Messages.Add(new Message(printLabelResult.GetConcatFailedOrdersString(), MessageTypes.Error));
                    }
                }
            }

            return printLabelResult;
        }


        public PrintLabelResult PrintBatch(long batchId, 
            long companyId, 
            long? by)
        {
            var printLabelResult = new PrintLabelResult()
            {
                IsPrintStarted = false
            };

            var syncInfo = new DbSyncInformer(_dbFactory,
                    _log,
                    _time,
                    SyncType.PostagePurchase,
                    "",
                    MarketType.None,
                    JsonConvert.SerializeObject(new { batchId = batchId }));

            using (var db = _dbFactory.GetRWDb())
            {
                var when = _time.GetAppNowTime();
                var company = db.Companies.GetByIdWithSettingsAsDto(companyId);
                var companyAddress = new CompanyAddressService(company);

                var labelProviders = _serviceFactory.GetShipmentProviders(_log,
                    _time,
                    _dbFactory,
                    _weightService,
                    company.ShipmentProviderInfoList,
                    _defaultCustomType,
                    _labelDirectory,
                    _reserveDirectory,
                    _templateDirectory);

                var labelService = new LabelService(labelProviders, _log, _time, _dbFactory, _emailService, _pdfMaker, _addressService);

                CompanyHelper.UpdateBalance(db,
                    company,
                    labelProviders,
                    true,
                    when);

                var orderIds = db.OrderBatches.GetOrderIdsForBatch(
                    batchId,
                    OrderStatusEnumEx.AllUnshippedWithShipped //for composing pdf file with all 
                    );

                var batchDto = db.OrderBatches.GetAsDto(batchId);
                //Close batch
                db.OrderBatches.CloseBatch(batchId);

                SendBeforePrintNotificationMessage(batchDto.Name);
                
                var shippingList =
                    db.OrderShippingInfos.GetOrderInfoWithItems(_weightService,
                        orderIds.ToList(), 
                        SortMode.ByLocation,
                        unmaskReferenceStyle: false,
                        includeSourceItems: false).ToList();

                //NOTE: Update from address
                var dropShipperList = db.DropShippers.GetAllAsDto().ToList();
                foreach (var shipping in shippingList)
                {
                    shipping.ReturnAddress = dropShipperList.FirstOrDefault(ds => ds.Id == shipping.DropShipperId)?.GetAddressDto();
                }

                //NOTE: update phone if missing
                foreach (var shipping in shippingList)
                {
                    if (shipping.ShippingMethod.CarrierName == ShippingServiceUtils.FedexCarrier
                        && String.IsNullOrEmpty(shipping.ToAddress.FinalPhone))
                    {
                        shipping.ToAddress.Phone = company.Phone;
                        shipping.ToAddress.ManuallyPhone = company.Phone;
                    }
                }

                //NOTE: Sort for setting #
                shippingList = SortHelper.Sort(shippingList, SortMode.ByShippingMethodThenLocation).ToList();

                IList<long> removedOrderIds = new List<long>();
                if (!syncInfo.IsSyncInProgress())
                {
                    UpdateLabelPrintStatus(db, orderIds, shippingList);

                    removedOrderIds = RemoveOrdersWithIssue(db, orderIds, shippingList, printLabelResult);
                    if (removedOrderIds.Any())
                    {
                        var batchName = string.Format("Issues of {0} {1} orders", batchDto.Name,
                            removedOrderIds.Count);

                        db.OrderBatches.CreateBatch(batchName,
                            BatchType.PrintLabel,
                            removedOrderIds,
                            when,
                            by);

                        //NOTE: keep to final email notification
                        //shippingList = shippingList.Where(sh => !removedOrderIds.Contains(sh.OrderId)).ToList();
                    }

                    //Removed list
                    var removedList = shippingList.Where(sh => removedOrderIds.Contains(sh.OrderId)).ToList();
                    if (batchDto.LockDate.HasValue)
                    {
                        var historyRemovedOrderIds = db.OrderChangeHistories.GetAll().Where(ch => ch.FromValue == batchDto.Id.ToString()
                                && ch.FieldName == OrderHistoryHelper.AddToBatchKey
                                && ch.ChangeDate >= batchDto.LockDate)
                            .Select(ch => ch.OrderId)
                            .Distinct()
                            .ToList();

                        historyRemovedOrderIds = historyRemovedOrderIds.Where(id => !orderIds.Contains(id)).ToList();
                        var historyShippingList = db.OrderShippingInfos.GetOrderInfoWithItems(_weightService,
                                historyRemovedOrderIds.ToList(),
                                SortMode.ByLocation,
                                unmaskReferenceStyle: false,
                                includeSourceItems: false).ToList();
                        removedList.AddRange(historyShippingList);
                    }

                    //Changed list
                    var styleChangedList = new List<StyleChangeInfo>();
                    if (batchDto.LockDate.HasValue)
                    {
                        var historyChangeStyles = db.OrderChangeHistories.GetAll().Where(ch => orderIds.Contains(ch.OrderId)
                               && ch.FieldName == OrderHistoryHelper.ReplaceItemKey
                               && ch.ChangeDate >= batchDto.LockDate)
                           .ToList();
                        foreach (var change in historyChangeStyles)
                        {
                            if (!String.IsNullOrEmpty(change.FromValue)
                                && !String.IsNullOrEmpty(change.ToValue))
                            {
                                var fromStyleItemId = StringHelper.ToInt(change.FromValue);
                                var fromStyleItem = db.StyleItems.GetAll().FirstOrDefault(si => si.Id == fromStyleItemId);
                                var toStyleItemId = StringHelper.ToInt(change.ToValue);
                                var toStyleItem = db.StyleItems.GetAll().FirstOrDefault(si => si.Id == toStyleItemId);
                                if (fromStyleItem != null
                                    && toStyleItem != null)
                                {
                                    var fromStyle = db.Styles.Get(fromStyleItem.StyleId);
                                    var toStyle = db.Styles.Get(toStyleItem.StyleId);

                                    styleChangedList.Add(new StyleChangeInfo()
                                    {
                                        SourceStyleString = fromStyle.StyleID,
                                        SourceStyleSize = fromStyleItem.Size,
                                        DestStyleString = toStyle.StyleID,
                                        DestStyleSize = toStyleItem.Size,
                                    });
                                }
                            }                            
                        }
                    }

                    //Printing
                    printLabelResult = labelService.PrintLabels(db,
                        company,
                        companyAddress,
                        syncInfo,
                        batchId,
                        shippingList,
                        false,
                        removedList,
                        styleChangedList,
                        batchDto.ScanFormPath,
                        _labelDirectory,
                        _isSampleMode,
                        when,
                        by);
                    printLabelResult.RemovedIds = removedOrderIds;

                    long? failedBatchId = null;
                    //Move orders with errors to a new batch
                    if (printLabelResult.FailedIds.Count > 0)
                    {
                        var batchName = string.Format("Failed of {0} {1} orders", batchDto.Name,
                            printLabelResult.FailedIds.Count);

                        var failedOrderIds = printLabelResult.FailedIds.Select(s => s.OrderId).Distinct().ToList();
                        failedBatchId = db.OrderBatches.CreateBatch(batchName,
                            BatchType.PrintLabel,
                            failedOrderIds,
                            when,
                            by);
                        printLabelResult.Messages.Add(
                            new Message(
                                String.Format("{0} unprinted orders was moved to new batch \"{1}\"",
                                    failedOrderIds.Count, batchName), MessageTypes.Error));
                    }

                    if (printLabelResult.IsPrintStarted)
                    {
                        //Send notification to seller
                        SendAfterPrintNotificationMessage(db,
                            printLabelResult.Messages,
                            batchDto.Id,
                            batchDto.Name,
                            company,
                            shippingList.Where(sh => printLabelResult.RemovedIds.Any(f => f == sh.OrderId)).ToList(),
                            shippingList.Where(sh => printLabelResult.FailedIds.Any(f => f.ShipmentId == sh.Id)).ToList());

                        //Apply new balance to Session
                        CompanyHelper.UpdateBalance(db,
                            company,
                            labelProviders,
                            false,
                            _time.GetAppNowTime());

                        SaveBatchPrintResultToDb(db,
                            _log,
                            batchDto.Id,
                            printLabelResult,
                            shippingList,
                            _time.GetAppNowTime(),
                            by);

                        if (printLabelResult.FailedIds.Any())
                        {
                            printLabelResult.Messages.Add(new Message(printLabelResult.GetConcatFailedOrdersString(),
                                MessageTypes.Error));
                        }
                    }
                    else
                    {
                        _emailService.SendSystemEmailToAdmin(String.Format("Batch \"{0}\" print wasn't started", batchDto.Name),
                            String.Format("Ended at {0}", _time.GetAppNowTime()));
                    }
                }
                else
                {
                    printLabelResult.Messages.Add(new Message("Request rejected. The system is already buys postage", MessageTypes.Warning));
                }

                return printLabelResult;
            }
        }

        private void UpdateLabelPrintStatus(IUnitOfWork db,
            long[] orderIds,
            IList<OrderShippingInfoDTO> shippingList)
        {
            //Remove OnHold orders from batch
            var onHoldShippingList = shippingList.Where(sh => sh.OnHold).ToList();
            if (onHoldShippingList.Any())
            {
                foreach (var shipping in onHoldShippingList)
                {
                    shipping.LabelPrintStatus = (int)LabelPrintStatus.OnHold;
                    _log.Info("set OnHold label print status, orderId=" + shipping.OrderAmazonId);
                }
            }

            //Removed with Mail label
            var alreadyMailedBatchOrderIds = db.MailLabelInfos.GetAllAsDto().Where(m => orderIds.Contains(m.OrderId)
                && !m.LabelCanceled
                && !m.CancelLabelRequested)
                .Select(m => m.OrderId).ToList();
            var alreadyMailedShippingList = shippingList.Where(sh => alreadyMailedBatchOrderIds.Contains(sh.OrderId)).ToList();
            if (alreadyMailedShippingList.Any())
            {
                foreach (var shipping in alreadyMailedShippingList)
                {
                    shipping.LabelPrintStatus = (int)LabelPrintStatus.AlreadyMailed;
                    _log.Info("Set Already mailed label print status, orderId=" + shipping.OrderAmazonId);
                }
            }
        }

        private IList<long> RemoveOrdersWithIssue(IUnitOfWork db,
            long[] orderIds,
            IList<OrderShippingInfoDTO> shippingList,
            PrintLabelResult printLabelResult)
        {
            var removedOrderIds = new List<long>();

            //Remove orders w/o shippings
            var orderIdsWoShippings = orderIds.Where(oId => shippingList.All(sh => sh.OrderId != oId)).ToList();
            if (orderIdsWoShippings.Any())
            {
                foreach (var orderId in orderIdsWoShippings)
                {
                    var dbOrder = db.Orders.GetById(orderId);
                    _batchManager.CheckRemovedOrder(db,
                        _log,
                        _actionService,
                        dbOrder,
                        null,
                        null);
                    dbOrder.BatchId = null;
                    _orderHistoryService.AddRecord(dbOrder.Id, OrderHistoryHelper.AddToBatchKey, dbOrder.BatchId, null, null, null, null);
                    removedOrderIds.Add(dbOrder.Id);
                    _log.Info("W/o shipping order was removed from batch, orderId=" + dbOrder.AmazonIdentifier);
                }
                printLabelResult.Messages.Add(
                    new Message(String.Format("{0} - orders w/o selected shipping was removed from batch",
                            orderIdsWoShippings.Distinct().Count()), MessageTypes.Error));
            }

            //Remove OnHold orders from batch
            var onHoldShippingList = shippingList.Where(sh => sh.OnHold).ToList();
            if (onHoldShippingList.Any())
            {
                foreach (var shipping in onHoldShippingList)
                {
                    var dbOrder = db.Orders.GetById(shipping.OrderId);
                    _batchManager.CheckRemovedOrder(db,
                        _log,
                        _actionService,
                        dbOrder,
                        null,
                        null);
                    dbOrder.BatchId = null;
                    _orderHistoryService.AddRecord(dbOrder.Id, OrderHistoryHelper.AddToBatchKey, dbOrder.BatchId, null, null, null, null);
                    removedOrderIds.Add(dbOrder.Id);
                    _log.Info("OnHold order was removed from batch, orderId=" + dbOrder.AmazonIdentifier);
                }
                printLabelResult.Messages.Add(new Message(String.Format("{0} on hold orders was removed from batch",
                            onHoldShippingList.Select(sh => sh.OrderId).Distinct().Count()), MessageTypes.Error));
            }

            //Removed with Mail label
            var alreadyMailedBatchOrderIds = db.MailLabelInfos.GetAllAsDto().Where(m => orderIds.Contains(m.Id)
                && !m.LabelCanceled
                && !m.CancelLabelRequested)
                .Select(m => m.OrderId).ToList();
            var alreadyMailedShippingList = shippingList.Where(sh => alreadyMailedBatchOrderIds.Contains(sh.OrderId)).ToList();
            if (alreadyMailedShippingList.Any())
            {
                foreach (var shipping in alreadyMailedShippingList)
                {
                    shipping.LabelPrintStatus = (int)LabelPrintStatus.AlreadyMailed;

                    var dbOrder = db.Orders.GetById(shipping.OrderId);
                    _orderHistoryService.AddRecord(dbOrder.Id, OrderHistoryHelper.AddToBatchKey, dbOrder.BatchId, null, null, null, null);
                    dbOrder.BatchId = null;
                    removedOrderIds.Add(dbOrder.Id);
                    _log.Info("Already mailed order was removed from batch, orderId=" + dbOrder.AmazonIdentifier);
                }
                printLabelResult.Messages.Add(new Message(String.Format("{0} previously processed and mailed orders was removed from batch",
                            alreadyMailedShippingList.Select(sh => sh.OrderId).Distinct().Count()), MessageTypes.Error));
            }

            db.Commit();

            return removedOrderIds;
        }

        private void SendAfterPrintNotificationMessage(IUnitOfWork db,
            IList<Message> messages,
            long batchId,
            string batchName,
            CompanyDTO company,            
            IList<OrderShippingInfoDTO> removedLabels,
            IList<OrderShippingInfoDTO> printErrorLabels)
        {
            try
            {
                if (removedLabels.Any() || printErrorLabels.Any())
                {
                    var removedOrderIds = removedLabels.Select(l => l.OrderAmazonId).Distinct().ToList();
                    var printErrorOrderIds = printErrorLabels.Select(l => l.OrderAmazonId).Distinct().ToList();

                    var subject = String.Format("Print batch \"{0}\" issues", batchName);

                    var body = @"<div>Order items removed from batch:</div>";

                    var items = removedLabels.SelectMany(o => o.Items).ToList();
                    items.AddRange(printErrorLabels.SelectMany(o => o.Items).ToList());
                    items = items.GroupBy(i => new {i.StyleId, i.StyleSize}).Select(g => new DTOOrderItem()
                    {
                        StyleId = g.Key.StyleId,
                        StyleSize = g.Key.StyleSize,
                        Quantity = g.Sum(i => i.Quantity)
                    }).ToList();

                    body += @"<table style='text-align:left; padding: 8px 0px 0px 10px; font-size: 11pt; font-family: Calibri'>
                        <tr><td style='width: 140px; background: #eee'>Style</td><td style='width: 50px; background: #eee'>Size</td><td style='width: 45px; background: #eee'>Qty</td></tr>";
                    foreach (var item in items)
                    {
                        body += "<tr><td>" + item.StyleId + "</td><td>" + item.StyleSize + "</td><td>" + item.Quantity +
                                "</td></tr>";
                    }
                    body += "</table>";


                    body += @"<br/>
                        <br/>
                        <div>Order numbers:</div>
                        <ul style='margin-top: 10px'>";
                    if (printErrorLabels.Any())
                    {
                        body += @"<li>
	                        <div>Moved to new batch (have print errors):</div>
	                        <ul>";
                        foreach (var orderNumber in printErrorOrderIds)
                        {
                            body += "<li>" + orderNumber + "</li>";
                        }
                        body += "</ul></li>";
                    }


                    if (removedLabels.Any())
                    {
                        body += @"<li>
                            <div>Moved to order page (onHold, w/o rates):</div>
                            <ul>";
                        foreach (var orderNumber in removedOrderIds)
                        {
                            body += "<li>" + orderNumber + "</li>";
                        }
                        body += "</ul></li>";
                    }
                    body += "</ul>";

                    _log.Info("Removed orders=" + removedOrderIds.Count + ", print errors=" + printErrorOrderIds.Count);

                    _emailService.SendSystemEmail(subject,
                        body,
                        StringHelper.Join(";", _config.PrintErrorsToEmails),
                        StringHelper.Join(";", _config.PrintErrorsCCEmails));
                        //StringHelper.JoinTwo(";", company.SellerEmail, company.SellerWarehouseEmailAddress),
                        //EmailHelper.SvetaEmail + ";" + EmailHelper.SupportDgtexEmail);

                }
                else
                {
                    var messageString = String.Join("<br/>", messages.Select(m => "[" + m.Type + "]: " + m.Text).ToList());

                    if (messages.Any(m => m.Type == MessageTypes.Error || m.Type == MessageTypes.Fatal))
                    {
                        _emailService.SendSystemEmailToAdmin(String.Format("Batch \"{0}\" printed with errors", batchName),
                            String.Format("Finished at {0}", _time.GetAppNowTime())
                            + "<br/>Messages: " + messageString);
                    }
                    else
                    {
                        _emailService.SendSystemEmailToAdmin(String.Format("Batch \"{0}\" printed successfully", batchName),
                            String.Format("Finished at {0}", _time.GetAppNowTime())
                            + "<br/>Messages: " + messageString);
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Info("Compose/Send notification error", ex);
            }
        }

        private void SendBeforePrintNotificationMessage(string batchName)
        {
            try
            {
                var now = _time.GetAppNowTime();

                var subject = String.Format("Print of the batch started \"{0}\"", batchName);
                var body = String.Format(@"Started at: {0}", now);

                _emailService.SendSystemEmailToAdmin(subject, body);
            }
            catch (Exception ex)
            {
                _log.Info("Compose/Send notification error", ex);
            }
        }


        private void SaveBatchPrintResultToDb(IUnitOfWork db,
            ILogService log,
            long? batchId,
            PrintLabelResult printResult,
            IList<OrderShippingInfoDTO> shippings,
            DateTime when,
            long? by)
        {
            //Create print pack file
            long? printPackId = null;

            if (!String.IsNullOrEmpty(printResult.Url))
            {
                var printPack = new LabelPrintPack
                {
                    CreateDate = DateHelper.GetAppNowTime(),
                    FileName = printResult.Url,
                    NumberOfLabels = shippings.Count,
                    BatchId = batchId,
                    PersonName = shippings.Count == 1
                        ? shippings.First().PersonName
                        : null
                };
                db.LabelPrintPacks.Add(printPack);
                db.Commit();

                printPackId = printPack.Id;
            }

            printResult.PrintPackId = printPackId;


            //Update link to print pack file for shippings
            if (printPackId.HasValue)
            {
                foreach (var shipping in shippings)
                {
                    var dbShipping = db.OrderShippingInfos.Get(shipping.Id);
                    if (dbShipping == null)
                        throw new Exception("dbShipping obj is NULL, dbShippingId=" + shipping.Id + ", orderId=" + shipping.OrderId);

                    dbShipping.LabelPrintPackId = printPackId;
                }
                db.Commit();
            }

            if (printResult.ResaveNumberInBatchRequested)
            {
                foreach (var shipping in shippings)
                {
                    var dbShipping = db.OrderShippingInfos.Get(shipping.Id);
                    if (dbShipping == null)
                        throw new Exception("dbShipping obj is NULL, dbShippingId=" + shipping.Id + ", orderId=" + shipping.OrderId);

                    dbShipping.NumberInBatch = shipping.NumberInList;
                }
                db.Commit();
            }

            if (printResult.PickupInfo != null)
            {
                try
                {
                    db.ScheduledPickups.Store(printResult.PickupInfo,
                        when,
                        by);
                }
                catch (Exception ex)
                {
                    log.Error("Store Pickup info", ex);
                }

            }

            //Update Batch Info
            if (batchId.HasValue)
            {
                var batch = db.OrderBatches.Get(batchId.Value);
                batch.LablePrintPackId = printResult.PrintPackId;
                if (printResult.ScanFormList != null)
                {
                    //NOTE: otherwise (if scan form id IS NULL) scan from path not changed
                    if (printResult.ScanFormList.Any(f => !String.IsNullOrEmpty(f.ScanFormId)))
                    {
                        batch.ScanFormPath = String.Join(";", printResult.ScanFormList.Select(s => s.ScanFormPath).ToList());
                        batch.ScanFormId = String.Join(";", printResult.ScanFormList.Select(s => s.ScanFormId).ToList());
                    }
                }
                if (printResult.PickupInfo != null)
                {
                    batch.PickupConfirmationNumber = printResult.PickupInfo.ConfirmationNumber;
                    batch.PickupTime = printResult.PickupInfo.ReadyByTime;
                    batch.PickupDate = DateHelper.CheckDateRange(printResult.PickupInfo.PickupDate);
                }

                db.Commit();
            }
        }


        public void ProcessPrintBatchActions(Action startPrintCallback)
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var printActions = _actionService.GetUnprocessedByType(db, SystemActionType.PrintBatch, null, null)
                    .OrderByDescending(a => a.CreateDate)
                    .ToList();
                _log.Info("Unprocessed actions: " + printActions.Count);

                foreach (var action in printActions)
                {
                    if (action.Status == (int) SystemActionStatus.None)
                    {
                        startPrintCallback?.Invoke();

                        _actionService.SetResult(db, action.Id, SystemActionStatus.InProgress, null);

                        var input = JsonConvert.DeserializeObject<PrintBatchInput>(action.InputData);

                        var result = PrintBatch(input.BatchId, input.CompanyId, input.UserId);
                        
                        _actionService.SetResult(db, action.Id, SystemActionStatus.Done, new PrintBatchOutput()
                        {
                            IsProcessed = true,
                            Messages = result.Messages,
                            FilePath = result.Url,
                            PrintPackId = result.PrintPackId,
                            PickupInfo = result.PickupInfo,
                        });

                        _log.Debug(String.Format(
                                "Print label action was done, batchId={0}, companyId={1}, userId={2}",
                                input.BatchId,
                                input.CompanyId,
                                input.UserId));
                    }
                }
                db.Commit();
            }
        }
    }
}
