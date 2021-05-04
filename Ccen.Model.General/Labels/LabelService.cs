using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.Core.Models.Dhl;
using Amazon.Core.Models.Settings;
using Amazon.Core.Models.Stamps;
using Amazon.DTO;
using Amazon.DTO.Contracts;
using Amazon.DTO.Orders;
using Amazon.DTO.Shippings;
using Amazon.DTO.Users;
using Amazon.Model.General.Markets;
using Amazon.Model.Implementation.Labels;
using Amazon.Model.Implementation.Pdf;
using ShipFIMS.Api;

namespace Amazon.Model.Implementation
{
    public class LabelService : ILabelService
    {
        private IList<IShipmentApi> _labelProviders;
        private ILogService _log;
        private IEmailService _emailService;
        private ITime _time;
        private IDbFactory _dbFactory;
        private IFileMaker _pdfMaker;
        private IAddressService _addressService;

        public LabelService(IList<IShipmentApi> labelProviders,
            ILogService log,
            ITime time,
            IDbFactory dbFactory,
            IEmailService emailService,
            IFileMaker pdfMaker,
            IAddressService addressService)
        {
            _labelProviders = labelProviders;
            _log = log;
            _time = time;
            _dbFactory = dbFactory;

            _emailService = emailService;
            _pdfMaker = pdfMaker;
            _addressService = addressService;
        }

        public CancelLabelResult CancelLabel(ShipmentProviderType providerType,
            string shipmentIdentifier,
            bool sampleMode)
        {
            _log.Info("LabelService.CancelLabel, providerType=" + providerType 
                + ", shipmentIdentifier=" + shipmentIdentifier 
                + ", sampleMode=" + sampleMode);

            if (sampleMode)
            {
                return new CancelLabelResult()
                {
                    IsSuccess = false,
                    Message = "Sample Mode"
                };
            }

            var provider = _labelProviders.FirstOrDefault(l => l.Type == providerType);

            if (provider != null)
            {
                try
                {
                    return provider.CancelShipment(shipmentIdentifier);
                }
                catch (Exception ex)
                {
                    return new CancelLabelResult()
                    {
                        IsSuccess = false,
                        Message = ex.Message
                    };
                }
            }
            return new CancelLabelResult()
            {
                IsSuccess = false,
                Message = "Not found shipment provider, type=" + providerType
            };
        }

        public PrintLabelResult PrintLabels(
            IUnitOfWork db,
            CompanyDTO company,
            ICompanyAddressService companyAddress,
            ISyncInformer syncInfo,
            long? batchId,
            IList<OrderShippingInfoDTO> shippingList,            
            bool skipScanForm,
            IList<OrderShippingInfoDTO> removedList,
            IList<StyleChangeInfo> styleChangedList,
            string existScanFormPath,
            string outputDirectory,
            bool sampleMode,
            DateTime? when,
            long? by)
        {
            _log.Info("PrintLabels begin, orders:");
            //foreach (var shipping in shippingList)
            //{
            //    _log.Info(shipping.OrderId.ToString());
            //    foreach (var item in shipping.Items)
            //    {
            //        _log.Info("Item.id=" + item.ItemOrderId + ", qty=" + item.Quantity);
            //    }
            //}

            var result = new PrintLabelResult();
            result.Success = true;

            var batchDto = batchId.HasValue ? db.OrderBatches.GetAsDto(batchId.Value) : null;

            var shipDate = db.Dates.GetOrderShippingDate(null);
            _log.Info("OriginalShipDate=" + shipDate);
            var now = _time.GetAppNowTime();
            if (shipDate.Date == now.Date
                && now.TimeOfDay < TimeSpan.FromHours(5))
            {
                if (_time.IsBusinessDay(now.AddDays(-1)))
                {
                    shipDate = shipDate.AddDays(-1);
                }
            }
            _log.Info("CorrectedShipDate=" + shipDate);

            var allWithNumbers = shippingList.All(sh => sh.NumberInBatch.HasValue);
            var allWithoutNumbers = shippingList.All(sh => !sh.NumberInBatch.HasValue);

            result.ResaveNumberInBatchRequested = !allWithNumbers && !allWithoutNumbers;

            var allPrintedLabels = new List<OrderShippingInfoDTO>();
            var getLabels = new List<OrderShippingInfoDTO>();

            for (int i = 0; i < shippingList.Count; i++)
            {
                var shipping = shippingList[i];

                if (allWithNumbers)
                    shipping.NumberInList = shipping.NumberInBatch ?? (i + 1); //NOTE: already exist
                else
                    shipping.NumberInList = i + 1;

                if (shipping.StampsShippingCost.HasValue 
                    && String.IsNullOrEmpty(shipping.LabelPath)
                    && (shipping.LabelPrintStatus == (int)LabelPrintStatus.None
                        || shipping.LabelPrintStatus == (int)LabelPrintStatus.PrintError))
                {
                    getLabels.Add(shipping);
                }
                else
                {
                    allPrintedLabels.Add(shipping);
                }
            }

            List<long> orderIdListWithIssue;
            if (batchId.HasValue)
            {
                //if (!CheckItems(db, shippingList, batchId.Value, out orderIdListWithIssue))
                //{
                //    var orderNumberListWithIssue = db.Orders.GetAll().Where(o => orderIdListWithIssue.Contains(o.Id))
                //        .Select(o => o.AmazonIdentifier)
                //        .ToList();

                //    result.Success = false;
                //    result.Messages.Add(new Message("Some orders have not passed validation of items count. Please try to recalculate them OR contact the support. Order list: "
                //                                    + String.Join(", ", orderNumberListWithIssue), MessageTypes.Fatal));
                //    result.Url = null;
                //    return result;
                //}
            }

            //foreach (var labelProvider in _labelProviders)
            //{
            //    if (!CheckBalance(getLabels, labelProvider))
            //    {
            //        result.Success = false;
            //        result.Messages.Add(new Message("Stamps account doesn't have enough money", MessageTypes.Fatal));
            //        result.Url = null;
            //        return result;
            //    }
            //}


            syncInfo.SyncBegin(getLabels.Count);
            if (batchId.HasValue)
            {
                _log.Info("Batch. SetPrintStatus, status=" + BatchPrintStatuses.InProgress);
                db.OrderBatches.SetPrintStatus(batchId.Value, BatchPrintStatuses.InProgress);
            }

            //Printing / Build PDF
            try
            {
                result.IsPrintStarted = true;

                var newLabels = GetLabels(companyAddress,
                    syncInfo,
                    shipDate,
                    getLabels,
                    result,
                    by,
                    sampleMode);

                newLabels = ReprintLabelWithErrorByStampsProvider(db, 
                    newLabels,
                    (labels) => GetLabels(companyAddress,
                        syncInfo,
                        shipDate,
                        labels,
                        result,
                        by,
                        sampleMode),
                    result);

                allPrintedLabels.AddRange(newLabels);

                MarkPartialPrintedOrdersAsUnprinted(allPrintedLabels, result); //NOTE: use allPrinted in case when complex order already partial printed (require to remove printed labels from already printed list)

                var successPrintedLabels = newLabels.Where(l => l.LabelPurchaseResult == (int) LabelPurchaseResultType.Success).ToList();
                var failedPrintLabels = newLabels.Where(sh => result.FailedIds.Any(f => f.ShipmentId == sh.Id)).ToList();

                #region Book Dhl pickup (Disable)
                //var successPrintedDhlLabels = successPrintedLabels.Where(l => l.ShipmentProviderType == (int) ShipmentProviderType.Dhl).ToList();
                //if (successPrintedDhlLabels.Any())
                //{
                //    var dhlProvider = _labelProviders.FirstOrDefault(l => l.Type == ShipmentProviderType.Dhl);
                //    if (dhlProvider != null)
                //    {
                //        try
                //        {
                //            var possibleScheduleTimes = new List<DhlScheduleTime>()
                //            {
                //                new DhlScheduleTime()
                //                {
                //                    PickupDate = db.Dates.GetOrderShippingDate(new TimeSpan(16, 0, 0)),
                //                    ReadyByTime = new TimeSpan(15, 0, 0),
                //                    CloseTime = new TimeSpan(16, 0, 0)
                //                },
                //                new DhlScheduleTime()
                //                {
                //                    PickupDate = db.Dates.GetOrderShippingDate(new TimeSpan(16, 30, 0)),
                //                    ReadyByTime = new TimeSpan(15, 30, 0),
                //                    CloseTime = new TimeSpan(16, 30, 0)
                //                },
                //                new DhlScheduleTime()
                //                {
                //                    PickupDate = db.Dates.GetOrderShippingDate(new TimeSpan(17, 0, 0)),
                //                    ReadyByTime = new TimeSpan(16, 0, 0),
                //                    CloseTime = new TimeSpan(17, 0, 0)
                //                },
                //                new DhlScheduleTime()
                //                {
                //                    PickupDate = db.Dates.GetOrderShippingDate(new TimeSpan(17, 30, 0)),
                //                    ReadyByTime = new TimeSpan(16, 30, 0),
                //                    CloseTime = new TimeSpan(17, 30, 0)
                //                },
                //            };

                //            var scheduleTimes = new List<DhlScheduleTime>();
                //            var now = _time.GetAppNowTime();
                //            foreach (var scheduleTime in possibleScheduleTimes)
                //            {
                //                if (scheduleTime.PickupDate == now.Date
                //                    && scheduleTime.ReadyByTime < now.TimeOfDay)
                //                {
                //                    //Skipped, if scheduled time earlier then current time
                //                }
                //                else
                //                {
                //                    scheduleTimes.Add(scheduleTime);
                //                }
                //            }

                //            var totalDhlWeight = successPrintedDhlLabels.Sum(l => l.WeightD);
                //            SheduleDhlPickup(db, 
                //                result,
                //                scheduleTimes,
                //                totalDhlWeight,
                //                pickupAddress,
                //                dhlProvider,
                //                (message, scheduleTime) =>
                //                {
                //                    var emailInfo = new DhlPickupScheduleErrorEmailInfo(
                //                        (batchId ?? 0).ToString(),
                //                        message,
                //                        scheduleTime.PickupDate,
                //                        scheduleTime.ReadyByTime,
                //                        scheduleTime.CloseTime,
                //                        company.SellerName,
                //                        company.SellerEmail);

                //                    _emailService.SendEmail(emailInfo);
                //                    _log.Info(String.Format("Send DHL pickup request scheduling error email, readyByTime: {0}, closeTime: {1}, pickupDate: {2}, batchId: {3},<br/>Message: {4}",
                //                        scheduleTime.ReadyByTime.ToString("hh\\:mm"),
                //                        scheduleTime.CloseTime.ToString("hh\\:mm"),
                //                        scheduleTime.PickupDate.ToString("MM/dd/yyyy"),
                //                        batchId,
                //                        message));
                //                },
                //                when,
                //                sampleMode);

                //        }
                //        catch (Exception ex)
                //        {
                //            _log.Fatal("Pickup error", ex);
                //        }
                //    }
                //}
                #endregion

                if (allPrintedLabels.Any())
                {
                    //NOTE: Second sort after part of labels was added after printed
                    allPrintedLabels = allPrintedLabels.OrderBy(p => p.CustomLabelSortOrder ?? p.NumberInList).ToList();

                    #region Get Scan Form

                    List<ScanFormInfo> scanForms = new List<ScanFormInfo>();
                    if (!skipScanForm)
                    {
                        var scanFormLabels = successPrintedLabels
                            .Where(l => !String.IsNullOrEmpty(l.StampsTxId)
                                && (l.ShipmentProviderType == (int)ShipmentProviderType.Stamps
                                  || l.ShipmentProviderType == (int)ShipmentProviderType.StampsPriority))
                            .ToList();

                        if (!sampleMode && scanFormLabels.Any())
                        {
                            var stampsProvider = _labelProviders.FirstOrDefault(p => p.Type == ShipmentProviderType.Stamps);

                            var getScanFormResult = GetScanForm(stampsProvider,
                                companyAddress.GetReturnAddress(MarketIdentifier.Empty()),
                                scanFormLabels.Select(l => new KeyValuePair<string, ShipmentProviderType>(l.StampsTxId, (ShipmentProviderType)l.ShipmentProviderType)).ToList(),
                                scanFormLabels.Max(l => l.ShippingDate) ?? DateTime.Today);

                            if (getScanFormResult.Status == CallStatus.Fail)
                            {
                                result.Messages.Add(
                                    new Message(
                                        "Scan form can't be processed/downloaded. Details: " +
                                        getScanFormResult.Message,
                                        MessageTypes.Warning));
                            }
                            else
                            {
                                scanForms = getScanFormResult.Data.ToList();
                            }
                        }
                    }

                    if (scanForms == null || scanForms.Count == 0)
                    {
                        var formPathList = StringHelper.Split(existScanFormPath, ";");
                        scanForms = formPathList.Select(f => new ScanFormInfo()
                        {
                            ScanFormPath = f
                        }).ToList();
                    }

                    #endregion
                    
                    #region Get Closeout Form

                    //IList<ScanFormInfo> closeoutForms = new List<ScanFormInfo>();
                    //if (!skipCloseoutForm)
                    //{
                    //    var closeoutLabels = successPrintedLabels
                    //        .Where(l => !String.IsNullOrEmpty(l.StampsTxId)
                    //            && l.ShipmentProviderType == (int)ShipmentProviderType.DhlECom)
                    //        .ToList();

                    //    if (!sampleMode && closeoutLabels.Any())
                    //    {
                    //        var stampsProvider = _labelProviders.FirstOrDefault(p => p.Type == ShipmentProviderType.DhlECom);

                    //        var getScanFormResult = GetCloseoutForm(stampsProvider,
                    //            closeoutLabels.Select(l => l.StampsTxId).ToList());

                    //        if (getScanFormResult.Status == CallStatus.Fail)
                    //        {
                    //            result.Messages.Add(
                    //                new Message(
                    //                    "Closeout form can't be processed/downloaded. Details: " +
                    //                    getScanFormResult.Message,
                    //                    MessageTypes.Warning));
                    //        }
                    //        else
                    //        {
                    //            closeoutForms = getScanFormResult.Data;
                    //        }
                    //    }
                    //}

                    #endregion

                    //scanForms.AddRange(closeoutForms);
                    result.ScanFormList = scanForms;

                    var hasFIMS = shippingList.Any(sh => sh.ShippingMethod.CarrierName == ShippingServiceUtils.FIMSCarrier);
                    var fimsProvider = _labelProviders.FirstOrDefault(pr => pr.Type == ShipmentProviderType.FIMS);
                    var fimsAirBillNumber = fimsProvider != null ? ((FIMSShipmentApi)fimsProvider).AirBillNumber : "";

                    //Compose PDF
                    BuildPdfFile(allPrintedLabels,
                        scanForms.Select(s => s.ScanFormPath).ToList(),
                        batchDto != null ? new BatchInfoToPrint()
                        {
                            BatchId = batchDto.Id,
                            BatchName = batchDto.Name,
                            Date = _time.GetAmazonNowTime(),
                            NumberOfPackages = allPrintedLabels.Count,
                            OrdersWithPrintError = failedPrintLabels,
                            OrdersWasManuallyRemoved = removedList,
                            Carriers = shippingList.GroupBy(sh => sh.ShippingMethod.CarrierName)
                                .Select(c => new {
                                    Key = c.Key,
                                    Value = c.Count()
                                })
                                .ToDictionary(d => d.Key, d => d.Value),
                            FIMSAirBillNumber = hasFIMS ? fimsAirBillNumber : "",
                            StyleChanges = styleChangedList,
                        } 
                        : null,
                        outputDirectory,
                        ref result);

                    if (result.CostDiff > 0)
                    {
                        result.Success = false;
                        result.Messages.Add(new Message(String.Format("Spent {0} more money than expected!", result.CostDiff), MessageTypes.Warning));
                    }
                    if (result.DuplicateCount > 0)
                    {
                        result.Messages.Add(new Message(String.Format("{0} labels have already been purchased, currently only printed", result.DuplicateCount), MessageTypes.Warning));
                    }
                }
            }
            finally
            {
                syncInfo.SyncEnd();
                if (batchId.HasValue)
                {
                    _log.Info("Batch. SetPrintStatus, status=" + BatchPrintStatuses.Printed);
                    db.OrderBatches.SetPrintStatus(batchId.Value, BatchPrintStatuses.Printed);
                }
            }

            return result;
        }

        private IList<OrderShippingInfoDTO> ReprintLabelWithErrorByStampsProvider(IUnitOfWork db,
            IList<OrderShippingInfoDTO> labels,
            Func<IList<OrderShippingInfoDTO>, IList<OrderShippingInfoDTO>> printCallback,
            PrintLabelResult result)
        {
            //Exclude: SameDay and Prime, we can't reprint it by stamps
            var toReprintCandidate = labels.Where(l => l.LabelPurchaseResult == (int) LabelPurchaseResultType.Error
                                              && l.ShipmentProviderType == (int) ShipmentProviderType.Amazon
                                              && l.ShippingMethodId != ShippingUtils.DynamexPTPSameShippingMethodId
                                              && l.OrderType != (int)OrderTypeEnum.Prime
                                              && ((l.LabelPurchaseMessage ?? "").Contains("ShipmentAlreadyExists")
                                                || (l.ShippingMethodId == ShippingUtils.AmazonFirstClassShippingMethodId //NOTE: Reprint Amazon FirstClass as Stamps FirstClass
                                                    && (l.LabelPurchaseMessage ?? "").Contains("ShippingServiceNotAvailable"))))
                .ToList();

            if (toReprintCandidate.Any())
            {
                var toReprint = new List<OrderShippingInfoDTO>();
                foreach (var label in toReprintCandidate)
                {
                    var stampsShippingMethodId = ShippingUtils.ConvertAmazonToStampsShippingMethod(label.ShippingMethod.Id);
                    if (stampsShippingMethodId.HasValue)
                    {
                        var method = db.ShippingMethods.GetByIdAsDto(stampsShippingMethodId.Value);
                        label.ShippingMethod = method;
                        label.ShipmentProviderType = (int) ShipmentProviderType.Stamps;
                        toReprint.Add(label);

                        _log.Info("Added to reprint list, shipmentId=" + label.Id + ", orderId=" + label.OrderId + "/" +
                                  label.OrderAmazonId);
                    }
                    else
                    {
                        _log.Info("Not added to reprint, hasn't stamps shippng method analog");
                    }
                }

                if (toReprint.Any())
                {
                    _log.Info("Reprint labels, count=" + toReprint.Count());
                    var body = "Following labels have the ShipmentAlreadyExists/ShippingServiceNotAvailable issue: <br/>";
                    body += String.Join("<br/>", toReprint.Select(l => l.OrderAmazonId + " - " + l.NumberInList + " - " + l.Id).ToList());

                    _emailService.SendSystemEmailToAdmin("Labels were reprinted", body);

                    //Correct result info
                    result.FailedIds = result.FailedIds.Where(e => toReprint.All(l => l.Id != e.ShipmentId)).ToList();

                    //Correct new labels

                    var allLabels = labels.Where(l => toReprint.All(p => p.Id != l.Id)).ToList();
                    var newLabels = printCallback(toReprint);
                    allLabels.AddRange(newLabels);

                    return allLabels;
                }
            }
            else
            {
                _log.Info("No labels to reprint");
            }

            return labels;
        }

        private void MarkPartialPrintedOrdersAsUnprinted(IList<OrderShippingInfoDTO> labels, PrintLabelResult result)
        {
            var withErrors = labels.Where(l => l.LabelPurchaseResult == (int) LabelPurchaseResultType.Error).ToList();
            var errorOrderList = withErrors.Where(l => l.OrderId != 0).Select(l => l.OrderId).ToList();
            var labelToUpdate = labels.Where(l => errorOrderList.Contains(l.OrderId)).ToList();
            foreach (var label in labelToUpdate)
            {
                if (label.LabelPurchaseResult != (int) LabelPurchaseResultType.Error)
                {
                    _log.Info("Reset label info, orderId=" + label.OrderId + ", numberInBatch=" + label.NumberInList);
                    label.LabelPurchaseResult = (int) LabelPurchaseResultType.Error;
                    label.StampsTxId = null;
                    label.LabelPath = null;

                    result.AddFailedLabel(label.Id, label.OrderId, label.OrderAmazonId, label.LabelPurchaseMessage ?? "none");
                }
            }
        }


        private class DhlScheduleTime
        {
            public DateTime PickupDate { get; set; }
            public TimeSpan ReadyByTime { get; set; }
            public TimeSpan CloseTime { get; set; }
        }

        private void SheduleDhlPickup(IUnitOfWork db,
            PrintLabelResult result,
            IList<DhlScheduleTime> scheduleTimes,
            double totalDhlWeight,
            AddressDTO pickupAddress,
            IShipmentApi dhlProvider,
            Action<string, DhlScheduleTime> failCallback,
            DateTime? when,
            bool sampleMode)
        {
            for (int i = 0; i < scheduleTimes.Count; i++)
            {
                var scheduleTime = scheduleTimes[i];

                _log.Info("Begin book pickup, weight=" + totalDhlWeight);

                result.PickupInfo = new ScheduledPickupDTO();
                result.PickupInfo.ProviderType = (int)ShipmentProviderType.Dhl;
                result.PickupInfo.RequestPickupDate = scheduleTime.PickupDate;
                result.PickupInfo.RequestReadyByTime = scheduleTime.ReadyByTime;
                result.PickupInfo.RequestCloseTime = scheduleTime.CloseTime;
                result.PickupInfo.SendRequestDate = when;

                CallResult<BookPickupInfo> pickupResult = null;
                var lastDhlPickup = db.ScheduledPickups.GetLast(ShipmentProviderType.Dhl);
                if (lastDhlPickup == null
                    || lastDhlPickup.RequestPickupDate.Date < scheduleTime.PickupDate.Date)
                {
                    _log.Info("Begin book pickup, weight=" + totalDhlWeight);

                    pickupResult = dhlProvider.BookPickup(scheduleTime.PickupDate,
                        scheduleTime.ReadyByTime,
                        scheduleTime.CloseTime,
                        totalDhlWeight,
                        pickupAddress,
                        sampleMode);
                }
                else
                {
                    _log.Info(String.Format("Pickup on date={0} already schedule",
                        scheduleTime.PickupDate.ToString("yyyy-MM-dd")));

                    return;
                }
                
                if (pickupResult.Status == CallStatus.Success)
                {
                    var data = pickupResult.Data;

                    result.PickupInfo.ConfirmationNumber = data.ConfirmationNumber;
                    result.PickupInfo.PickupDate = data.PickupDate;
                    result.PickupInfo.ReadyByTime = data.ReadyByTime;
                    result.PickupInfo.PickupCharge = data.PickupCharge;
                    result.PickupInfo.CallInTime = data.CallinTime;

                    _log.Info("ConfirmationNumber=" + data.ConfirmationNumber
                              + ", ReadyByTime=" + data.ReadyByTime
                              + ", PickupDate=" + data.PickupDate);

                    break;
                }
                else
                {
                    if (i == scheduleTimes.Count - 1) //Last schedule fail, send email and save results
                    {
                        result.PickupInfo.ResultMessage = pickupResult.Message;

                        result.Messages.Add(new Message("The pickup request has error: " + pickupResult.Message, MessageTypes.Warning));

                        failCallback(pickupResult.Message, scheduleTime);
                    }
                }
            }
        }


        public PrintLabelResult PrintMailLabel(IUnitOfWork db,
            IShippingService shippingService,
            MailLabelDTO model,
            DateTime when,
            long? by,
            string outputDirectory,
            string templateDirectory,
            bool sampleMode)
        {
            var printResult = new PrintLabelResult();
            printResult.Success = true;

            _log.Info("PrintMailLabel, model=" + model + Environment.NewLine + "outputDirectory=" + outputDirectory);

            try
            {
                var toAddress = model.ToAddress;
                var returnAddress = model.FromAddress;
                var pickupAddress = model.FromAddress;

                var labelProvider = _labelProviders.FirstOrDefault(p => (int)p.Type == model.ShipmentProviderType);

                var shipmentInfo = new OrderShippingInfoDTO()
                {
                    OrderAmazonId = model.OrderId,
                    WeightD = (model.WeightLb ?? 0)*16 + (model.WeightOz ?? 0),
                    PackageLength = model.PackageLength,
                    PackageWidth = model.PackageWidth,
                    PackageHeight = model.PackageHeight,
                    IsInsured = model.IsInsured,
                    TotalPrice = model.TotalPrice,
                    TotalPriceCurrency = model.TotalPriceCurrency,

                    IsSignConfirmation = model.IsSignConfirmation,

                    Items = model.Items,
                    ShippingMethod = model.ShippingMethod,
                };

                var boughtInTheCountry = model.BoughtInTheCountry;

                var shipDate = db.Dates.GetOrderShippingDate(null);

                var callResult = labelProvider.CreateShipment(shipmentInfo,
                    returnAddress,
                    pickupAddress,
                    toAddress,
                    boughtInTheCountry,
                    shipDate.Date,
                    model.Notes,
                    !model.ShippingMethod.IsSupportReturnToPOBox,
                    sampleMode,
                    fromUI: true);

                if (callResult.Status == CallStatus.Success)
                {
                    var data = callResult.Data;

                    model.EstimatedDeliveryDate = data.EarliestEstimatedDeliveryDate;
                    model.ShippingDate = data.ShipDate;
                    model.TrackingNumber = data.TrackingNumber;
                    model.IntegratorTxIdentifier = data.IntegratorShipmentId;
                    model.StampsTxId = data.ProviderShipmentId;

                    if (data.RateAmount > 0) //Otherwise keep current (requested by inexpress)
                        model.StampsShippingCost = data.RateAmount;
                    model.SignConfirmationCost = data.SignConfirmationCost;
                    model.InsuranceCost = data.InsuranceCost;
                    model.UpChargeCost = shippingService.GetUpCharge(model.ShippingMethod.Id, data.RateAmount);


                    var personName = model.IsAddressSwitched ? model.FromAddress.FinalFullName : model.ToAddress.FinalFullName;
                    var labelInfoList = new List<PrintLabelInfo>();
                    foreach (var label in data.LabelFileList)
                    {
                        labelInfoList.Add(new PrintLabelInfo
                        {
                            Number = 1,
                            BatchId = 0,
                            Image = label.AbsoluteFilePath,
                            RelativeImagePath = label.RelativeFilePath,
                            OrderId = string.IsNullOrEmpty(model.OrderId) ? " " : model.OrderId,
                            PersonName = personName,
                            ServiceType = ShippingUtils.GetShippingType(model.ShippingMethod.Id),
                            PackageType = ShippingUtils.GetPackageType(model.ShippingMethod.Id),
                            ShippingMethodId = model.ShippingMethod.Id,
                            Duplicate = false,
                            LabelSize = model.ShippingMethod.IsFullPagePrint ? PrintLabelSizeType.FullPage : PrintLabelSizeType.HalfPage,
                            RotationAngle = model.ShippingMethod.RotationAngle,
                            Notes = model.Notes,
                        });
                    }

                    

                    LabelPrintPack printPack = null;
                    if (model.Reason == (int)MailLabelReasonCodes.ReturnLabelReasonCode
                        && labelInfoList.Count == 1) //Only if one image, return it
                    {
                        var firstLabelImage = labelInfoList[0].RelativeImagePath;

                        printPack = new LabelPrintPack
                        {
                            CreateDate = DateHelper.GetAppNowTime(),
                            FileName = firstLabelImage,
                            NumberOfLabels = 1,
                            PersonName = personName,
                            IsReturn = model.IsAddressSwitched
                        };
                    }
                    else
                    {
                        //TASK: Don’t print mini pick list on return labels
                        var isReturn = model.Reason == (int)MailLabelReasonCodes.ReturnLabelReasonCode;

                        if (!isReturn || !String.IsNullOrEmpty(model.Instructions))
                        {
                            //Compose Mail Pick List
                            var miniPickListInfo = BuildMailPickListPdf(model.Items,
                                model.TrackingNumber,
                                !isReturn,
                                model.Instructions,
                                outputDirectory,
                                templateDirectory);
                            if (miniPickListInfo != null)
                            {
                                labelInfoList.Add(new PrintLabelInfo()
                                {
                                    Image = miniPickListInfo.AbsoluteFilePath,
                                    LabelSize = PrintLabelSizeType.HalfPage,
                                    SpecialType = LabelSpecialType.MailPickList,
                                });
                            }
                        }

                        var pdfFileName = _pdfMaker.CreateFileWithLabels(labelInfoList, 
                            null, 
                            null,
                            outputDirectory);
                        printPack = new LabelPrintPack
                        {
                            CreateDate = DateHelper.GetAppNowTime(),
                            FileName = pdfFileName,
                            NumberOfLabels = 1,
                            PersonName = personName,
                            IsReturn = model.IsAddressSwitched
                        };
                    }

                    db.LabelPrintPacks.Add(printPack);
                    db.Commit();

                    var labelPathes = String.Join(";", data.LabelFileList.Select(l => l.RelativeFilePath).ToList());

                    model.LabelPath = labelPathes;
                    model.LabelPrintPackId = printPack.Id;

                    db.MailLabelInfos.StoreInfo(model, model.Items, when, by);
                    
                    printResult.PrintPackId = printPack.Id;
                    printResult.Url = printPack.FileName;
                    printResult.TrackingNumber = data.TrackingNumber;
                    printResult.Carrier = shipmentInfo.ShippingMethod.CarrierName;
                }
                else
                {
                    printResult.Success = false;
                    printResult.Messages.Add(new Message(callResult.Message, MessageTypes.Fatal));

                    if (callResult is CallReqResResult<GetLabelDTO>
                        && labelProvider.Type == ShipmentProviderType.SkyPostal)
                    {
                        _emailService?.SendSystemEmailToAdmin("SkyPostal Print Label Error " + model.OrderId,
                            "Request: " + (callResult as CallReqResResult<GetLabelDTO>).RequestBody
                            + "Response: " + (callResult as CallReqResResult<GetLabelDTO>).ResponseBody);
                    }                    
                }
            }
            catch (Exception ex)
            {
                _log.Error("Error in PrintMailLabel", ex);
                printResult.Success = false;
                printResult.Messages.Add(new Message("Unable to print label, reason: " + ExceptionHelper.GetMostDeeperException(ex).Message, MessageTypes.Fatal));
            }

            return printResult;
        }

        private LabelFileInfo BuildMailPickListPdf(IList<DTOOrderItem> items, 
            string trackingNumber,
            bool inlcudeItems,
            string instructions,
            string outputDirectory,
            string templateDirectory)
        {
            try
            {
                var pickListFileName = "~\\Labels\\mailPickList_" + trackingNumber + "_" +
                                      DateTime.UtcNow.Ticks + ".pdf";
                var pickListFilePath = outputDirectory + pickListFileName.Trim(new[] { '~' });
                var pickListData = MiniPickListPdf.Build(templateDirectory,
                    trackingNumber,
                    items,
                    inlcudeItems,
                    instructions);
                
                FileHelper.WriteToFile(pickListData, pickListFilePath);

                return new LabelFileInfo()
                {
                    AbsoluteFilePath = pickListFilePath,
                    RelativeFilePath = pickListFileName,
                };
            }
            catch (Exception ex)
            {
                _log.Error("Generate Mini Pick List invoice", ex);

                return null;
            }
        }

        private void InnerPurchaseLabel(OrderShippingInfoDTO info,
            AddressDTO returnAddress,
            AddressDTO pickupAddress,
            string boughtInTheCountry,
            ISyncInformer syncInfo,
            DateTime shipDate,
            PrintLabelResult result,
            long? by,
            bool sampleMode)
        {
            try
            {
                _log.Info(string.Format("Order {0}: Expected cost: {1}, Market: {2}", info.OrderId, info.StampsShippingCost, info.Market));
                
                var provider = _labelProviders.FirstOrDefault(p => (int)p.Type == info.ShipmentProviderType);

                var notes = String.IsNullOrEmpty(info.CustomNotes) ? ShippingUtils.BuildLabelNotes(info) : info.CustomNotes;
                var hasPickup = !info.ShippingMethod.IsSupportReturnToPOBox;

                decimal newBalance;

                var expectedShippingCost = info.StampsShippingCost;

                //PRINT
                var shippingInfo = GetLabel(provider,
                    shipDate,
                    returnAddress,
                    pickupAddress,
                    boughtInTheCountry,
                    info,
                    notes,
                    hasPickup,
                    by,
                    sampleMode,
                    out newBalance);

                info.LabelPath = shippingInfo.LabelPath;
                info.StampsTxId = shippingInfo.StampsTxId;
                info.ShippingDate = shippingInfo.ShippingDate;
                info.LabelPurchaseDate = shippingInfo.LabelPurchaseDate;
                info.TrackingNumber = shippingInfo.TrackingNumber;
                info.StampsShippingCost = shippingInfo.StampsShippingCost;
                info.IntegratorTxIdentifier = shippingInfo.IntegratorTxIdentifier;

                info.LabelPrintStatus = (int)LabelPrintStatus.Printed;
                info.LabelPurchaseResult = shippingInfo.LabelPurchaseResult;
                info.LabelPurchaseMessage = shippingInfo.LabelPurchaseMessage;

                provider.SetBalance(newBalance);

                if (expectedShippingCost.HasValue
                    && info.StampsShippingCost.HasValue
                    && expectedShippingCost.Value < info.StampsShippingCost.Value - 1)
                {
                    var errorMessage = "Spent more money: $"
                        + PriceHelper.RoundToTwoPrecision(expectedShippingCost) + " -> $"
                        + PriceHelper.RoundToTwoPrecision(info.StampsShippingCost);

                    info.LabelPurchaseResult = (int)LabelPurchaseResultType.Error;
                    info.LabelPurchaseMessage = errorMessage;
                    info.LabelPrintStatus = (int)LabelPrintStatus.PrintError;

                    using (var db = _dbFactory.GetRWDb())
                    {
                        var dbShipping = db.OrderShippingInfos.Get(info.Id);

                        if (dbShipping != null)
                        {
                            dbShipping.NumberInBatch = info.NumberInList;
                            dbShipping.LabelPurchaseResult = info.LabelPurchaseResult;
                            dbShipping.LabelPurchaseMessage = info.LabelPurchaseMessage;
                            db.Commit();
                        }
                    }

                    result.AddFailedLabel(info.Id, info.OrderId, info.OrderAmazonId, errorMessage);
                }
            }
            catch (Exception ex)
            {
                _log.Info("Error Into GetLabel", ex);

                var purchaseResult = (int)LabelPurchaseResultType.Error;
                var purchaseMessage = StringHelper.Substring(ex.Message, 512);
                try
                {
                    using (var db = _dbFactory.GetRWDb())
                    {
                        var dbShipping = db.OrderShippingInfos.Get(info.Id);
                        if (dbShipping == null)
                            throw new Exception(String.Format("During printing the order shipment info were changed (no order shipping with Id = {0})",
                                    info.Id));

                        dbShipping.NumberInBatch = info.NumberInList;
                        dbShipping.LabelPurchaseResult = purchaseResult;
                        dbShipping.LabelPurchaseMessage = purchaseMessage;
                        db.Commit();
                    }
                }
                catch (Exception exInCatch)
                {
                    _log.Error("Error Into GetLabel (catch part), can't write failed info into shipment", exInCatch);
                }

                info.LabelPurchaseResult = purchaseResult;
                info.LabelPurchaseMessage = purchaseMessage;
                info.LabelPrintStatus = (int) LabelPrintStatus.PrintError;

                result.Success = false;
                result.AddFailedLabel(info.Id, info.OrderId, info.OrderAmazonId, ex.Message);

                _log.Error(string.Format("Unable get label for order: {0}", info.OrderId), ex);
                
                syncInfo.AddError(info.Id.ToString(), "Unable get label for order", ex);
            }
            syncInfo.UpdateProgress(info.Id.ToString());
        }

        private IList<OrderShippingInfoDTO> GetLabels(
            ICompanyAddressService companyAddress,
            ISyncInformer syncInfo, 
            DateTime shipDate,
            IList<OrderShippingInfoDTO> shippingInfos, 
            PrintLabelResult result,
            long? by,
            bool sampleMode)
        {
            ILabelPurchaser purchaser = null;
            if (shippingInfos.Select(sh => sh.OrderAmazonId).Distinct().Count() > 5)
            {
                purchaser = new MultiThreadPurchaser(_log);
            }
            else
            {
                purchaser = new SingleThreadPurchaser(_log);
            }

            purchaser.PurshaseLabels(shippingInfos, (info) => InnerPurchaseLabel(info,
                    info.ReturnAddress ?? companyAddress.GetReturnAddress(info.GetMarketId()),
                    companyAddress.GetPickupAddress(info.GetMarketId()),
                    MarketBaseHelper.GetMarketCountry((MarketType)info.Market, info.MarketplaceId),
                    syncInfo,
                    shipDate,
                    result,
                    by,
                    sampleMode));

            result.CostDiff = purchaser.ActualCost - purchaser.ExpectedCost;
            _log.Info("Cost difference: " + result.CostDiff + ", actual=" + purchaser.ActualCost + ", expected=" + purchaser.ExpectedCost);

            return shippingInfos;
        }

        private OrderShippingInfo GetLabel(
            IShipmentApi shipmentProvider,
            DateTime shipDate,
            AddressDTO returnAddress,
            AddressDTO pickupAddress,
            string boughtInTheCountry,
            OrderShippingInfoDTO shippingInfo,
            string notes,
            bool hasPickup,
            long? by,
            bool sampleMode,
            out decimal newBalance)
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var toAddress = db.Orders.GetAddressInfo(shippingInfo.OrderId);

                if (toAddress == null)
                    toAddress = shippingInfo.ToAddress;

                //returnAddress.FullName = _addressService.DefaultName; //TODO: Get by shipping Market
                returnAddress.IsVerified = true;

                //pickupAddress.FullName = _addressService.DefaultName;
                pickupAddress.IsVerified = true;

                var callResult = shipmentProvider.CreateShipment(shippingInfo,
                    returnAddress,
                    pickupAddress,
                    toAddress,
                    boughtInTheCountry,
                    shipDate,
                    notes,
                    hasPickup,
                    sampleMode,
                    fromUI: false);

                if (callResult.Status == CallStatus.Fail)
                {
                    if (callResult is CallReqResResult<GetLabelDTO>
                            && shipmentProvider.Type == ShipmentProviderType.SkyPostal)
                    {
                        _emailService?.SendSystemEmailWithAttachments("SkyPostal Print Label Error " + shippingInfo.OrderAmazonId,
                            "",
                            new Attachment[]
                            {
                                Attachment.CreateAttachmentFromString((callResult as CallReqResResult<GetLabelDTO>).RequestBody, "RequestBody"),
                                Attachment.CreateAttachmentFromString((callResult as CallReqResResult<GetLabelDTO>).ResponseBody, "ResponseBody"),
                            },
                            "ildar@dgtex.com",
                            null);
                    }

                    throw new Exception(callResult.Message);
                }
                else
                {
                    var data = callResult.Data;

                    data.ShipmentProviderType = (int) shipmentProvider.Type;

                    var now = DateHelper.GetAppNowTime();

                    var labelPaths = String.Join(";", data.LabelFileList.Select(l => l.RelativeFilePath).ToList());

                    //Update shipping info
                    OrderShippingInfo dbShipping = null;
                    if (shippingInfo.Id > 0)
                    {
                        dbShipping = db.OrderShippingInfos.Get(shippingInfo.Id);
                        if (dbShipping == null)
                        {
                            //NOTE: Try to restore shipping Id, after possible recalculation
                            var newActiveShippings = db.OrderShippingInfos.GetAll()
                                .Where(sh => sh.OrderId == shippingInfo.OrderId
                                    && sh.IsActive
                                    && sh.ShippingMethodId == shippingInfo.ShippingMethodId)
                                .ToList();
                            if (newActiveShippings.Count == 1)
                            {
                                _log.Info("dbShipping was restored: " + shippingInfo.Id + "=>" + dbShipping.Id);
                                dbShipping = newActiveShippings.FirstOrDefault();
                                shippingInfo.Id = dbShipping.Id;
                            }
                            if (newActiveShippings.Count > 1)
                            {
                                var activeShippingIds = newActiveShippings.Select(sh => sh.Id).ToList();
                                var activeShippingItems = db.ItemOrderMappings.GetAll().Where(ish => activeShippingIds.Contains(ish.ShippingInfoId)).ToList();
                                foreach (var activeShipping in newActiveShippings)
                                {
                                    var shippingItems = activeShippingItems.Where(sh => sh.ShippingInfoId == activeShipping.Id)
                                        .ToList();
                                    if (shippingItems.Count() == shippingInfo.Items.Count)
                                    {
                                        if (shippingItems.All(shi => shippingInfo.Items.Any(sourceSh => sourceSh.OrderItemEntityId == shi.OrderItemId)))
                                        {
                                            _log.Info("dbShipping was restored: " + shippingInfo.Id + "=>" + activeShipping.Id);
                                            dbShipping = activeShipping;
                                            shippingInfo.Id = activeShipping.Id;
                                        }
                                    }

                                }
                            }
                        }

                        if (dbShipping == null)
                        {
                            throw new Exception("System Error: Unable to store the label info. Please try to re-print the order.");
                        }
                    }
                    else
                    {
                        dbShipping = new OrderShippingInfo(); //NOTE: Create fake shipping info (for external orders)
                        dbShipping.OrderId = 0;
                        dbShipping.ShippingNumber = (db.OrderShippingInfos.GetAll().Where(sh => sh.OrderId == 0).Max(sh => sh.ShippingNumber) + 1)?? 1;
                        dbShipping.ShipmentProviderType = shippingInfo.ShipmentProviderType;
                        dbShipping.CreateDate = _time.GetAppNowTime();
                        dbShipping.NumberInBatch = shippingInfo.NumberInList;
                        dbShipping.ShipmentOfferId = shippingInfo.OrderAmazonId;
                        dbShipping.IsFulfilled = true;
                        db.OrderShippingInfos.Add(dbShipping);
                    }

                    if (dbShipping.NumberInBatch != shippingInfo.NumberInList) //NOTE: log if changed numberInBatch
                        _log.Info(String.Format("Updated numberInBatch, orderId={0}, from={1}, to={2}",
                            shippingInfo.OrderId,
                            dbShipping.NumberInBatch,
                            shippingInfo.NumberInList));

                    dbShipping.NumberInBatch = shippingInfo.NumberInList;

                    if (data.RateAmount > 0) //Otherwise keep current (requested by inexpress)
                        dbShipping.StampsShippingCost = data.RateAmount;
                    dbShipping.UsedWeight = data.Weight;

                    dbShipping.LabelPath = labelPaths;
                    dbShipping.LabelPurchaseDate = now;
                    dbShipping.LabelPurchaseResult = (int)LabelPurchaseResultType.Success;
                    dbShipping.LabelPurchaseMessage = "";
                    dbShipping.LabelPurchaseBy = by;

                    dbShipping.ShippingDate = data.ShipDate;
                    dbShipping.EstimatedDeliveryDate = data.LatestEstimatedDeliveryDate;
                    dbShipping.EarliestDeliveryDate = data.EarliestEstimatedDeliveryDate;
                    dbShipping.DeliveryDaysInfo = data.DeliveryDaysInfo;

                    var newDeliveryDays = _time.GetBizDaysCount(data.ShipDate, data.EarliestEstimatedDeliveryDate);
                    if (newDeliveryDays != dbShipping.DeliveryDays)
                        _log.Info(String.Format("DeliveryDays changed: {0} => {1}",
                            dbShipping.DeliveryDays,
                            newDeliveryDays));
                    dbShipping.DeliveryDays = newDeliveryDays;

                    dbShipping.TrackingNumber = data.TrackingNumber;
                    dbShipping.IntegratorTxIdentifier = data.IntegratorShipmentId;
                    dbShipping.StampsTxId = data.ProviderShipmentId;

                    dbShipping.UpdateDate = now;

                    db.Commit();

                    newBalance = data.NewAccountBalance ?? 0;

                    return dbShipping;
                }
            }
        }

        public void BuildPdfFile(IList<OrderShippingInfoDTO> orders,
            IList<string> scanFormPathList,
            BatchInfoToPrint batchInfo,
            string outputDirectory, 
            ref PrintLabelResult result)
        {
            var printLabels = orders;

            var labels = new List<PrintLabelInfo>();
            foreach (var printLabel in printLabels)
            {
                if (!String.IsNullOrEmpty(printLabel.LabelPath)
                    && (printLabel.LabelPrintStatus == (int)LabelPrintStatus.Printed
                        || printLabel.LabelPrintStatus == (int)LabelPrintStatus.None))
                {
                    var labelImages = printLabel.LabelPath.Split(";".ToCharArray());
                    foreach (var labelImage in labelImages)
                    {
                        labels.Add(new PrintLabelInfo
                        {
                            PrintResult = (LabelPrintStatus)printLabel.LabelPrintStatus,
                            Number = printLabel.NumberInList,
                            BatchId = (int)(printLabel.BatchId ?? 0),
                            Image = outputDirectory + labelImage.Trim(new[] {'~'}),
                            OrderId = OrderHelper.FormatOrderNumber(printLabel.OrderAmazonId, (MarketType)printLabel.Market),
                            PersonName = printLabel.PersonName,
                            Duplicate = printLabel.PrintLabelPackId.HasValue,
                            ServiceType = ShippingUtils.GetShippingType(printLabel.ShippingMethod.Id),
                            PackageType = ShippingUtils.GetPackageType(printLabel.ShippingMethod.Id),
                            
                            ShippingMethodId = printLabel.ShippingMethod.Id,
                            PackageNameOnLabel = printLabel.ShippingMethod.PackageNameOnLabel,
                            LabelSize = printLabel.ShippingMethod.IsFullPagePrint ? PrintLabelSizeType.FullPage : PrintLabelSizeType.HalfPage,
                            RotationAngle = printLabel.ShippingMethod.RotationAngle,
                            Notes = ShippingUtils.BuildLabelNotes(printLabel),
                        });
                    }
                }
                else
                {
                    labels.Add(new PrintLabelInfo
                    {
                        PrintResult = (LabelPrintStatus)printLabel.LabelPrintStatus,
                        Number = printLabel.NumberInList,
                        BatchId = (int)(printLabel.BatchId ?? 0),
                        Image = "",
                        OrderId = OrderHelper.FormatOrderNumber(printLabel.OrderAmazonId, (MarketType)printLabel.Market),
                        PersonName = printLabel.PersonName,
                        Duplicate = printLabel.PrintLabelPackId.HasValue,
                        ServiceType = ShippingUtils.GetShippingType(printLabel.ShippingMethod.Id),
                        PackageType = ShippingUtils.GetPackageType(printLabel.ShippingMethod.Id),

                        ShippingMethodId = printLabel.ShippingMethod.Id,
                        PackageNameOnLabel = printLabel.ShippingMethod.PackageNameOnLabel,
                        LabelSize = PrintLabelSizeType.HalfPage,
                        Notes = ShippingUtils.BuildLabelNotes(printLabel),
                    });
                }
            }

            result.DuplicateCount = labels.Count(l => l.Duplicate);
            //PRINT LABELS
            try
            {
                var pdfFileName = _pdfMaker.CreateFileWithLabels(labels,
                    scanFormPathList.Select(s => outputDirectory + s.Trim(new[] {'~'})).ToList(),
                    batchInfo,
                    outputDirectory);

                result.Url = pdfFileName;
            }
            catch (Exception ex)
            {
                result.Messages.Add(new Message("Error when generate pdf file, message=" + ex.Message, MessageTypes.Fatal));
            }
        }

        private bool CheckItems(IUnitOfWork db,
            IList<OrderShippingInfoDTO> shippingList,
            long batchId,
            out List<long> ordersWithIssue)
        {
            ordersWithIssue = new List<long>();

            var orderIds = db.OrderBatches.GetOrderIdsForBatch(
                    batchId,
                    OrderStatusEnumEx.AllUnshippedWithShipped);

            var batchDto = db.OrderBatches.GetAsDto(batchId);

            var items = shippingList.SelectMany(sh => OrderHelper.GroupBySourceItemOrderId(sh.Items)).ToList();
            var itemsCount = items.Sum(i => i.Quantity);

            var sourceItems = db.OrderItemSources.GetAllAsDto().Where(oi => orderIds.Contains(oi.OrderId)).ToList();
            var sourceItemsCount = sourceItems.Sum(i => i.QuantityOrdered);
            
            if (itemsCount != sourceItemsCount)
            {
                foreach (var orderId in orderIds)
                {
                    var orderItems = shippingList.Where(sh => sh.OrderId == orderId).SelectMany(sh => sh.Items).ToList();
                    orderItems = OrderHelper.GroupBySourceItemOrderId(orderItems);
                    var currentItemCount = orderItems.Sum(i => i.Quantity);

                    var sourceItemCount = sourceItems.Where(i => i.OrderId == orderId).Sum(i => i.QuantityOrdered);
                    if (currentItemCount != sourceItemCount)
                        ordersWithIssue.Add(orderId);
                }
                return false;
            }

            return true;
        }


        #region Balance
        private bool CheckBalance(IList<OrderShippingInfoDTO> orders,
            IShipmentApi shipmentProvider)
        {
            if (shipmentProvider.Type == ShipmentProviderType.Stamps
                || shipmentProvider.Type == ShipmentProviderType.StampsPriority)
            {
                var accountType = shipmentProvider.Type;

                var orderShippingCostSum = orders.Where(o => o.ShipmentProviderType == (int)accountType).Sum(o => o.StampsShippingCost ?? 0);
                var orderInsuranceCostSum = orders.Where(o => o.ShipmentProviderType == (int)accountType).Sum(o => o.IsInsured ? (o.InsuranceCost ?? 0) : 0);
                var orderSignCostSum = orders.Where(o => o.ShipmentProviderType == (int)accountType).Sum(o => o.IsSignConfirmation ? (o.SignConfirmationCost ?? 0) : 0);

                var totalSum = orderShippingCostSum + orderInsuranceCostSum + orderSignCostSum;

                var balance = shipmentProvider.Balance;


                _log.Info("CheckBalance, type=" + accountType + ": totalSum=" + totalSum + ", balance=" + balance + ", label count=" + orders.Count());
                return totalSum < balance;
            }

            return true;
        }

        public IList<AccountInfo> UpdateBalance(
            IUnitOfWork db,
            DateTime when)
        {
            IList<AccountInfo> results = new List<AccountInfo>();
            try
            {
                foreach (var provider in _labelProviders)
                {
                    var newBalance = provider.GetBalance();

                    if (newBalance != null)
                    {
                        db.ShipmentProviders.UpdateBalance(provider.ProviderId, newBalance.Balance, when);

                        _log.Debug("Balance updated, new balance=" + newBalance
                                   + ", providerId=" + provider.ProviderId);

                        results.Add(newBalance);
                    }
                    else
                    {
                        _log.Debug("Balance info [null] for provider=" + provider.ProviderId + ", type=" + provider.Type);
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error("Unable to get balance", ex);
            }
            return results;
        }
        #endregion


        #region Scan Form
        private CallResult<IList<ScanFormInfo>> GetScanForm(
            IShipmentApi labelProvider,
            AddressDTO fromAddress,
            IList<KeyValuePair<string, ShipmentProviderType>> stampsTxIds,
            DateTime shipDate)
        {
            _log.Info("GetScanForm, stampsTxIds=" + stampsTxIds + ", shipDate=" + shipDate);
            
            try
            {
                foreach (var txId in stampsTxIds)
                {
                    if (String.IsNullOrEmpty(txId.Key))
                    {
                        return CallResult<IList<ScanFormInfo>>.Fail("Empty one of stampsTxId value", null);
                    }
                }

                var addressFrom = new AddressDTO
                {
                    //NOTE: temporary forse setting Amazon fullname
                    FullName = _addressService.GetFullname(MarketType.Amazon),
                    Address1 = fromAddress.Address1,
                    City = fromAddress.City,
                    State = fromAddress.State,
                    Country = fromAddress.Country,
                    Zip = fromAddress.Zip,
                    Phone = fromAddress.Phone
                };

                try
                {
                    _log.Info("Before general scan form");
                    var result = labelProvider.GetScanForm(stampsTxIds.Select(s => s.Key).ToList(),
                        addressFrom,
                        shipDate);

                    return result;
                }
                catch (Exception ex)
                {
                    _log.Error("Can't print scan form, shipmentProviderIds=" + stampsTxIds, ex);

                    return CallResult<IList<ScanFormInfo>>.Fail(ex.Message, ex);
                }
            }
            catch (Exception ex)
            {
                _log.Error("GetScanForm", ex);

                return CallResult<IList<ScanFormInfo>>.Fail(ex.Message, ex);
            }
        }
        #endregion

        #region Closeout Form
        private CallResult<IList<ScanFormInfo>> GetCloseoutForm(
            IShipmentApi labelProvider,
            IList<string> packageIds)
        {
            _log.Info("GetCloseoutForm, packageIds=" + String.Join(", ", packageIds));

            try
            {
                foreach (var packageId in packageIds)
                {
                    if (String.IsNullOrEmpty(packageId))
                    {
                        return CallResult<IList<ScanFormInfo>>.Fail("Empty one of packageId value", null);
                    }
                }
                try
                {
                    _log.Info("Before general closeout form");
                    var result = labelProvider.GetScanForm(packageIds.ToList(),
                        null,
                        DateTime.MinValue);

                    return result;
                }
                catch (Exception ex)
                {
                    _log.Error("Can't print closeout form", ex);

                    return CallResult<IList<ScanFormInfo>>.Fail(ex.Message, ex);
                }
            }
            catch (Exception ex)
            {
                _log.Error("GetScanForm", ex);

                return CallResult<IList<ScanFormInfo>>.Fail(ex.Message, ex);
            }
        }
        #endregion
    }
}