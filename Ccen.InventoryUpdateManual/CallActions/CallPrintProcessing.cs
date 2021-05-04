using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Amazon.Api;
using Amazon.Api.AmazonECommerceService;
using Amazon.Common.Helpers;
using Amazon.Common.Models;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models;
using Amazon.Core.Models.Settings;
using Amazon.Core.Models.Stamps;
using Amazon.DAL;
using Amazon.DTO;
using Amazon.DTO.Shippings;
using Amazon.DTO.Users;
using Amazon.Model.General;
using Amazon.Model.Implementation;
using Amazon.Model.Implementation.Errors;
using Amazon.Model.Implementation.Labels;
using Amazon.Model.Implementation.Pdf;
using Amazon.Model.Implementation.Rates;
using Amazon.Model.Implementation.Sorting;
using Amazon.Web.ViewModels;


namespace Amazon.InventoryUpdateManual.CallActions
{
    public class CallPrintProcessing
    {
        private ILogService _log;
        private ITime _time;
        private CompanyDTO _company;
        private IEmailService _emailService;
        private IFileMaker _pdfMaker;
        private IDbFactory _dbFactory;
        private IWeightService _weightService;

        private string _outputDirectory;
        private string _templateDirectory;
        private string _reserveDirectory;

        public CallPrintProcessing(
            ILogService log,
            ITime time,
            IDbFactory dbFactory,
            IEmailService emailService,
            CompanyDTO company,
            string outputDirectory,
            string templateDirectory,
            string reserveDirectory)
        {
            _log = log;
            _time = time;
            _company = company;
            _emailService = emailService;
            _pdfMaker = new PdfMakerByIText(_log);
            _dbFactory = dbFactory;
            _weightService = new WeightService();

            _outputDirectory = outputDirectory;
            _templateDirectory = templateDirectory;
            _reserveDirectory = reserveDirectory;
        }

        public void PrintActionProcessing()
        {
            var orderHistoryService = new OrderHistoryService(_log, _time, _dbFactory);
            var batchManager = new BatchManager(_log, _time, orderHistoryService, _weightService);
            var actionService = new SystemActionService(_log, _time);
            var serviceFactory = new ServiceFactory();

            var labelBatchService = new LabelBatchService(_dbFactory,
                actionService,
                _log,
                _time,
                _weightService,
                serviceFactory,
                _emailService,                
                batchManager,
                _pdfMaker,
                AddressService.Default,
                orderHistoryService,
                AppSettings.DefaultCustomType,
                AppSettings.LabelDirectory,
                AppSettings.ReserveDirectory,
                AppSettings.TemplateDirectory,
                new LabelBatchService.Config(),
                AppSettings.IsSampleLabels);

            labelBatchService.ProcessPrintBatchActions(null);
        }

        public void AutoBuyAmazonNextDay()
        {
            var orderHistoryService = new OrderHistoryService(_log, _time, _dbFactory);
            var batchManager = new BatchManager(_log, _time, orderHistoryService, _weightService);
            var actionService = new SystemActionService(_log, _time);
            var serviceFactory = new ServiceFactory();

            var labelBatchService = new LabelBatchService(_dbFactory,
                actionService,
                _log,
                _time,
                _weightService,
                serviceFactory,
                _emailService,
                batchManager,
                _pdfMaker,
                AddressService.Default,
                orderHistoryService,
                AppSettings.DefaultCustomType,
                AppSettings.LabelDirectory,
                AppSettings.ReserveDirectory,
                AppSettings.TemplateDirectory,
                new LabelBatchService.Config(),
                AppSettings.IsSampleLabels);

            var autoPurchaseService = new LabelAutoBuyService(_dbFactory,
                _log,
                _time,
                batchManager,
                labelBatchService,
                actionService,
                _emailService,
                _weightService,
                _company.Id);

            autoPurchaseService.PurchaseAmazonNextDay();
        }

        public void AutoBuySameDay()
        {
            var orderHistoryService = new OrderHistoryService(_log, _time, _dbFactory);
            var batchManager = new BatchManager(_log, _time, orderHistoryService, _weightService);
            var actionService = new SystemActionService(_log, _time);
            var serviceFactory = new ServiceFactory();

            var labelBatchService = new LabelBatchService(_dbFactory,
                actionService,
                _log,
                _time,
                _weightService,
                serviceFactory,
                _emailService,
                batchManager,
                _pdfMaker,
                AddressService.Default,
                orderHistoryService,
                AppSettings.DefaultCustomType,
                AppSettings.LabelDirectory,
                AppSettings.ReserveDirectory,
                AppSettings.TemplateDirectory,
                new LabelBatchService.Config(),
                AppSettings.IsSampleLabels);

            var autoPurchaseService = new LabelAutoBuyService(_dbFactory,
                _log,
                _time,
                batchManager,
                labelBatchService,
                actionService,
                _emailService,
                _weightService,
                _company.Id);

            autoPurchaseService.PurchaseForSameDay();
        }

        public void AutoBuyOverdueOrders()
        {
            var orderHistoryService = new OrderHistoryService(_log, _time, _dbFactory);
            var batchManager = new BatchManager(_log, _time, orderHistoryService, _weightService);
            var actionService = new SystemActionService(_log, _time);
            var serviceFactory = new ServiceFactory();

            var labelBatchService = new LabelBatchService(_dbFactory,
                actionService,
                _log,
                _time,
                _weightService,
                serviceFactory,
                _emailService,
                batchManager,
                _pdfMaker,
                AddressService.Default,
                orderHistoryService,
                AppSettings.DefaultCustomType,
                AppSettings.LabelDirectory,
                AppSettings.ReserveDirectory,
                AppSettings.TemplateDirectory,
                new LabelBatchService.Config(),
                AppSettings.IsSampleLabels);

            var autoPurchaseService = new LabelAutoBuyService(_dbFactory,
                _log,
                _time,
                batchManager,
                labelBatchService,
                actionService,
                _emailService,
                _weightService,
                _company.Id);

            autoPurchaseService.PurchaseForOverdue();
        }

        public void GetAmazonShipment()
        {
            var providers = GetShipmentProviders(_company);
            var amazon = (AmazonShipmentApi)providers.FirstOrDefault(p => p.Type == ShipmentProviderType.Amazon);
            var shipment = amazon.GetShipment("e624ad6f-208b-4cbe-aae9-3898451fa25f");

        }

        private IList<IShipmentApi> GetShipmentProviders(CompanyDTO company)
        {
            var serviceFactory = new ServiceFactory();
            return serviceFactory.GetShipmentProviders(_log,
                _time,
                _dbFactory,
                _weightService,
                company.ShipmentProviderInfoList,
                AppSettings.DefaultCustomType,
                AppSettings.LabelDirectory,
                AppSettings.ReserveDirectory,
                AppSettings.TemplateDirectory);
        }
        
        public void GetBalance()
        {
            var labelService = new LabelService(GetShipmentProviders(_company), _log, _time, _dbFactory, _emailService, _pdfMaker, AddressService.Default);
            using (var db = new UnitOfWork(_log))
            {
                labelService.UpdateBalance(db, _time.GetAppNowTime());
            }
        }

        public bool Validate(long batchId)
        {
            var ordersWithIssue = new List<long>();
            using (var db = _dbFactory.GetRWDb())
            {
                var orderIds = db.OrderBatches.GetOrderIdsForBatch(
                    batchId,
                    OrderStatusEnumEx.AllUnshippedWithShipped //for composing pdf file with all 
                    );

                var batchDto = db.OrderBatches.GetAsDto(batchId);

                var shippingList =
                    db.OrderShippingInfos.GetOrderInfoWithItems(_weightService,
                        orderIds.ToList(), 
                        SortMode.ByLocation,
                        unmaskReferenceStyle: false,
                        includeSourceItems: false).ToList();

                var testOrderShippings = shippingList.Where(sh => sh.OrderId == 274753).ToList();
                _log.Info(testOrderShippings.Count.ToString());

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

        }

        public void RePrintBatch(long batchId)
        {
            var actionService = new SystemActionService(_log, _time);
            var serviceFactory = new ServiceFactory();
            var orderHistoryService = new OrderHistoryService(_log, _time, _dbFactory);
            var batchManager = new BatchManager(_log, _time, orderHistoryService, _weightService);

            var batchService = new LabelBatchService(_dbFactory,
                actionService,
                _log,
                _time,
                _weightService,
                serviceFactory,
                _emailService,
                batchManager,
                _pdfMaker,
                AddressService.Default,
                orderHistoryService,
                AppSettings.DefaultCustomType,
                AppSettings.LabelDirectory,
                AppSettings.ReserveDirectory,
                AppSettings.TemplateDirectory,
                new LabelBatchService.Config(),
                AppSettings.IsSampleLabels);

            var result = batchService.PrintBatch(batchId, _company.Id, null);
            _log.Info("Success: " + result.Success.ToString());
        }

        public void RePrintLastPack(long packId)
        {
            IList<long> orderIds;
            var labelService = new LabelService(GetShipmentProviders(_company), _log, _time, _dbFactory, _emailService, _pdfMaker, AddressService.Default);
            var companyAddress = new CompanyAddressService(_company);
            var addressService = new AddressService(new List<IAddressCheckService>(), companyAddress.GetReturnAddress(MarketIdentifier.Empty()), companyAddress.GetPickupAddress(MarketIdentifier.Empty()));
            using (var db = new UnitOfWork(_log))
            {
                //packId = 329;// db.GetSet<LabelPrintPack>().OrderByDescending(l => l.CreateDate).FirstOrDefault().Id;
//                var shippings = db.OrderShippingInfos.GetByLabelPackId(packId);
//                orderIds = shippings.Select(o => o.OrderId).ToList();
//#if DEBUG
//                //orderIds = orderIds.Take(3).ToList();
//#endif
//                labelService.PrintLabels(_log,
//                    db,
//                    addressService,
//                    _user,
//                    new EmptySyncInformer(SyncType.PostagePurchase),
//                    shippings.AsQueryable(),
//                    false,
//                    null,
//                    SortMode.ByLocation,
//                    AppSettings.LabelDirectory,
//                   AppSettings.IsSampleLabels);

//                string orderListAsString = String.Join(",", orderIds);
//                _log.Debug(orderListAsString);
            }
        }

        public void CancelBatch(long batchId)
        {
            var labelService = new LabelService(GetShipmentProviders(_company), _log, _time, _dbFactory, _emailService, _pdfMaker, AddressService.Default);
            using (var db = _dbFactory.GetRWDb())
            {
                var orderIds = (from o in db.Orders.GetAll()
                                join sh in db.OrderShippingInfos.GetAll() on o.Id equals sh.OrderId
                                where o.BatchId == batchId
                                   && !String.IsNullOrEmpty(sh.TrackingNumber)
                                   && !sh.CancelLabelRequested
                                select o.Id).ToList();
                orderIds = orderIds.Distinct().ToList();

                foreach (var orderId in orderIds)
                {
                    _log.Info("Cancelling order: " + orderId);
                    MailViewModel.CancelCurrentOrderLabels(_log, db, labelService, _time, orderId, false);
                }
            }
        }


        public void CallCanelLabelsRun1()
        {
            //var trackingService =
            //CallCancelLabels(new[] { "9400116901495496008512" });// "9400116901495496008512""C4605899-C36F-408D-BDD5-0B9F23F8C6E3" });

            var stampsTxIds = File.ReadAllLines(@"C:\Projects.Vionix\Marketplaces\MarketsSellerCentral\Amazon.InventoryUpdateManual\Files\StampsTxIdList2.txt");
            CallCancelLabels(stampsTxIds, ShipmentProviderType.Stamps);
        }

        public void CallCancelLabels(string[] trackingNumberList, ShipmentProviderType providerType)
        {
            var labelService = new LabelService(GetShipmentProviders(_company), _log, _time, _dbFactory, _emailService, _pdfMaker, AddressService.Default);

            foreach (var trackingNumber in trackingNumberList)
            {
                labelService.CancelLabel(providerType, trackingNumber, false);
            }
        }

        public void CallPrintSampleLabel(string orderId)
        {
            using (var db = new UnitOfWork(_log))
            {
                var order = db.Orders.GetFiltered(o => o.AmazonIdentifier == orderId).FirstOrDefault();
                //var labelService = new LabelService();
                //var addressService = new AddressService(new List<IAddressCheckService>());
                var orderIds = new List<long> { order.Id };



                //labelService.PrintLabels(_log,
                //    db,
                //    addressService,
                //    _user,
                //    new EmptySyncInformer(SyncType.PostagePurchase),
                //    orderIds.ToArray(),
                //    true,
                //    null,
                //    SortMode.ByLocation,
                //    AppSettings.LabelDirectory,
                //    AppSettings.IsSampleLabels);

                string orderListAsString = String.Join(",", orderIds);
                _log.Debug(orderListAsString);
            }
        }


        public void GetDhlInvoice()
        {
            using (var db = _dbFactory.GetRWDb())
            {
                var orderIdList = from o in db.Orders.GetAll()
                    join sh in db.OrderShippingInfos.GetAll() on o.Id equals sh.OrderId
                    where o.BatchId == 1907
                          && sh.IsActive
                          && (sh.ShippingMethodId == ShippingUtils.DhlExpressWorldWideShippingMethodId
                              || sh.ShippingMethodId == ShippingUtils.AmazonDhlExpressMXShippingMethodId)
                    select o.Id;

                var shippingList =
                    db.OrderShippingInfos.GetOrderInfoWithItems(_weightService,
                        orderIdList.ToList(),
                        SortMode.ByLocation,
                        unmaskReferenceStyle: false,
                        includeSourceItems: false).ToList();

                var companyAddress = new CompanyAddressService(_company);

                foreach (var shippingInfo in shippingList)
                {
                    var toAddress = db.Orders.GetAddressInfo(shippingInfo.OrderAmazonId);
                    var pickupAddress = companyAddress.GetPickupAddress(shippingInfo.GetMarketId());

                    _log.Info("Begin generate invoice");
                    try
                    {
                        var trackingNumber = shippingInfo.TrackingNumber;

                        var invoiceFileName = "~\\Labels\\invoice_" + trackingNumber + "_" +
                                              DateTime.UtcNow.Ticks + ".pdf";
                        var invoiceFilePath = _outputDirectory + invoiceFileName.Trim(new[] {'~'});
                        var commercialInvoice = CommercialInvoiceHelper.BuildCommercialInvoice(_templateDirectory,
                            shippingInfo.OrderAmazonId,
                            shippingInfo,
                            pickupAddress,
                            toAddress,
                            shippingInfo.Items,
                            trackingNumber,
                            shippingInfo.ShippingDate.Value);
                        try
                        {
                            var dbShippingInfo = db.OrderShippingInfos.Get(shippingInfo.Id);
                            var labels = (dbShippingInfo.LabelPath ?? "").Split(";".ToCharArray());
                            if (labels.Count() == 1)
                            {
                                FileHelper.WriteToFile(commercialInvoice, invoiceFilePath);

                                dbShippingInfo.LabelPath += ";" + invoiceFileName;
                                db.Commit();
                            }
                        }
                        catch (Exception ex)
                        {
                            //NOTE: continue all processing, label content we can get later using GetShipment
                            _log.Fatal("Can't save label, path=" + invoiceFilePath, ex);

                            var reserveInvoiceFilePath = _reserveDirectory + invoiceFileName.Trim(new[] {'~'});
                            try
                            {
                                FileHelper.WriteToFile(commercialInvoice, reserveInvoiceFilePath);
                            }
                            catch (Exception exInner)
                            {
                                _log.Fatal("Can't save invoice, path=" + reserveInvoiceFilePath, exInner);
                            }
                        }


                    }
                    catch (Exception ex)
                    {
                        _log.Error("Generate Commercial invoice", ex);
                    }
                }
            }
        }

        public void PrintPdfFromBatch(long batchId)
        {
            var labelService = new LabelService(GetShipmentProviders(_company), _log, _time, _dbFactory, _emailService, new PdfMakerByIText(_log), AddressService.Default);
            using (var db = new UnitOfWork(_log))
            {
                var orderIds = db.OrderBatches.GetOrderIdsForBatch(
                    batchId,
                    OrderStatusEnumEx.AllUnshippedWithShipped //for composing pdf file with all 
                    );

                var batchDto = db.OrderBatches.Get(batchId);

                var shippingList = db.OrderShippingInfos.GetOrderInfoWithItems(_weightService, orderIds.ToList(), (SortMode)SortMode.ByLocation, unmaskReferenceStyle: false, includeSourceItems:false)
                    .ToList();
                shippingList = SortHelper.Sort(shippingList, SortMode.ByShippingMethodThenLocation).Take(9).ToList().ToList();

                var result = new PrintLabelResult();
                labelService.BuildPdfFile(shippingList,
                    new string[] { },
                    new BatchInfoToPrint()
                    {
                        BatchId = batchId,
                        BatchName = batchDto.Name,
                        NumberOfPackages = shippingList.Count,
                        Date = _time.GetAmazonNowTime(),
                        Carriers = shippingList.GroupBy(sh => sh.ShippingMethod.CarrierName)
                            .Select(c => new {
                                Key = c.Key,
                                Value = c.Count()
                            })                        
                            .ToDictionary(d => d.Key, d => d.Value)
                    },
                    AppSettings.LabelDirectory, 
                    ref result);
            }
        }

        public void PrintPdfFromOrders()
        {
            var labelService = new LabelService(GetShipmentProviders(_company), _log, _time, _dbFactory, _emailService, new PdfMakerByIText(_log), AddressService.Default);
            using (var db = new UnitOfWork(_log))
            {
                var batchId = 1887;

                var orderIds = from o in db.Orders.GetAll()
                                  join sh in db.OrderShippingInfos.GetAll() on o.Id equals sh.OrderId
                                  where o.BatchId == batchId
                                        && sh.IsActive
                                        && (sh.ShippingMethodId == ShippingUtils.DhlExpressWorldWideShippingMethodId
                                            || sh.ShippingMethodId == ShippingUtils.AmazonDhlExpressMXShippingMethodId)
                                  select o.Id;

                var batchDto = db.OrderBatches.Get(batchId);

                var shippingList = db.OrderShippingInfos.GetOrderInfoWithItems(_weightService, orderIds.ToList(), (SortMode)SortMode.ByLocation, unmaskReferenceStyle: false, includeSourceItems: false)
                    .ToList();
                shippingList = SortHelper.Sort(shippingList, SortMode.ByShippingMethodThenLocation).ToList();

                var mailList = db.MailLabelInfos.GetAllAsDto().Where(m => orderIds.Contains(m.OrderId)).ToList();
                shippingList.AddRange(mailList);

                var result = new PrintLabelResult();
                labelService.BuildPdfFile(shippingList.Where(sh => !String.IsNullOrEmpty(sh.LabelPath)).ToList(),
                    new string[] { },
                    new BatchInfoToPrint()
                    {
                        BatchId = batchDto.Id,
                        BatchName = batchDto.Name,
                        Date = _time.GetAmazonNowTime(),
                        NumberOfPackages = shippingList.Count
                    },
                    AppSettings.LabelDirectory,
                    ref result);
            }
        }

        public void PrintPdfFromOrder(string amazonIdentifier)
        {
            var labelService = new LabelService(GetShipmentProviders(_company), _log, _time, _dbFactory, _emailService, new PdfMakerByIText(_log), AddressService.Default);
            using (var db = new UnitOfWork(_log))
            {
                var orderIds = from o in db.Orders.GetAll()
                               where o.AmazonIdentifier == amazonIdentifier
                               select o.Id;

                //var batchDto = db.OrderBatches.Get(734);

                var shippingList = db.OrderShippingInfos.GetOrderInfoWithItems(_weightService, orderIds.ToList(), (SortMode)SortMode.ByLocation, unmaskReferenceStyle: false, includeSourceItems: false)
                    .ToList();
                shippingList = SortHelper.Sort(shippingList, SortMode.ByShippingMethodThenLocation).ToList();

                var mailList = db.MailLabelInfos.GetAllAsDto().Where(m => orderIds.Contains(m.OrderId)).ToList();
                shippingList.AddRange(mailList);

                var result = new PrintLabelResult();
                labelService.BuildPdfFile(shippingList.Where(sh => !String.IsNullOrEmpty(sh.LabelPath)).ToList(),
                    new string[] { },
                    null,
                    AppSettings.LabelDirectory,
                    ref result);
            }
        }

        public void PrintScanForm()
        {
            var outputDirectory = AppSettings.LabelDirectory;
            var labelService = new LabelService(GetShipmentProviders(_company), _log, _time, _dbFactory, _emailService, _pdfMaker, AddressService.Default);

            var scanFormRelativePath = new string[] { "~\\ScanForms\\scanform_9475711201080820619182.jpg" };
            Console.WriteLine(scanFormRelativePath);

            var result = new PrintLabelResult();
            labelService.BuildPdfFile(new List<OrderShippingInfoDTO>(),
                scanFormRelativePath,
                null,
                outputDirectory,
                ref result);
        }

        public void GetCloseOutManifest(long? batchId)
        {
            var outputDirectory = AppSettings.LabelDirectory;
            var shipmentProviders = GetShipmentProviders(_company);
            var ibsProvider = shipmentProviders.First(p => p.Type == ShipmentProviderType.IBC);

            using (var db = new UnitOfWork(_log))
            {
                var stampsTxIDList = new List<string>();
                if (batchId.HasValue)
                {
                    var orderIds = db.OrderBatches.GetOrderIdsForBatch(
                        batchId.Value,
                        OrderStatusEnumEx.AllUnshippedWithShipped);

                    //var fromDate = new DateTime(2015, 12, 19, 11, 0, 0);// '2015-12-19 11:08:36.217'
                    var shippingList = db.OrderShippingInfos
                        .GetOrderInfoWithItems(_weightService, orderIds.ToList(), SortMode.ByLocation, unmaskReferenceStyle: false, includeSourceItems: false).ToList();

                    var stampsShippingList = shippingList
                        .Where(sh => !String.IsNullOrEmpty(sh.StampsTxId)
                                     && sh.ShipmentProviderType == (int)ShipmentProviderType.IBC)
                        .ToList();


                    stampsTxIDList = stampsShippingList.Select(s => s.StampsTxId).ToList();
                }
                else
                {
                    stampsTxIDList = new List<string>();
                }
                
                IList<ScanFormInfo> scanForms = null;
                string getScanFormMessage = "";

                var scanFormRelativePath = ibsProvider.GetScanForm(
                    stampsTxIDList.ToArray(),
                    null,
                    DateTime.Now);

                _log.Info("Path=" + scanFormRelativePath);
                Console.WriteLine(scanFormRelativePath);
            }
        }

        public void GetScanForm(long? batchId, DateTime shipDate)
        {
            var outputDirectory = AppSettings.LabelDirectory;
            var shipmentProviders = GetShipmentProviders(_company);
            var stampsProvider = shipmentProviders.First(p => p.Type == ShipmentProviderType.Stamps);

            using (var db = new UnitOfWork(_log))
            {
                DateTime? formShipDate = shipDate;// _time.GetAppNowTime().Date;
                var stampsTxIDList = new List<string>();
                if (batchId.HasValue)
                {
                    var orderIds = db.OrderBatches.GetOrderIdsForBatch(
                        batchId.Value,
                        OrderStatusEnumEx.AllUnshippedWithShipped);

                    //var fromDate = new DateTime(2015, 12, 19, 11, 0, 0);// '2015-12-19 11:08:36.217'
                    var shippingList = db.OrderShippingInfos
                        .GetOrderInfoWithItems(_weightService, orderIds.ToList(), SortMode.ByLocation, unmaskReferenceStyle: false, includeSourceItems:false).ToList();

                    var stampsShippingList = shippingList
                        .Where(sh => !String.IsNullOrEmpty(sh.StampsTxId)
                                     && (sh.ShipmentProviderType == (int) ShipmentProviderType.Stamps
                                         || sh.ShipmentProviderType == (int) ShipmentProviderType.StampsPriority))
                        .ToList();


                    formShipDate = stampsShippingList.Max(l => l.ShippingDate);

                    stampsTxIDList = stampsShippingList.Select(s => s.StampsTxId).ToList();
                }
                else
                {
                    stampsTxIDList = new List<string>();
                }

                var addressFrom = new AddressDTO
                {
                    //NOTE: temporary forse setting Amazon fullname
                    FullName = AddressService.Default.GetFullname(MarketType.Amazon),
                    Address1 = _company.Address1,
                    City = _company.City,
                    State = _company.State,
                    Country = _company.Country,
                    Zip = _company.Zip,
                    Phone = _company.Phone
                };

                IList<ScanFormInfo> scanForms = null;
                string getScanFormMessage = "";
                
                var scanFormRelativePath = stampsProvider.GetScanForm(
                    stampsTxIDList.ToArray(),
                    addressFrom,
                    formShipDate ?? DateTime.Today);

                _log.Info("Path=" + scanFormRelativePath);
                Console.WriteLine(scanFormRelativePath);
            }
        }
    }
}
