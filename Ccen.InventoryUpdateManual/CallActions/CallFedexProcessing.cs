using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities;
using Amazon.Core.Models;
using Amazon.Core.Models.Settings;
using Amazon.Core.Models.Stamps;
using Amazon.DTO;
using Amazon.DTO.Users;
using Amazon.Model.General;
using Amazon.Model.Implementation;
using Amazon.Utils;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.InventoryUpdateManual.CallActions
{
    public class CallFedexProcessing
    {
        private ILogService _log;
        private ITime _time;
        private IDbFactory _dbFactory;
        private CompanyDTO _company;

        public CallFedexProcessing(ILogService log,
            ITime time,
            IDbFactory dbFactory,
            CompanyDTO company)
        {
            _log = log;
            _time = time;
            _dbFactory = dbFactory;
            _company = company;
        }

        public void UpdateRateReport()
        {
            var adjustedFilepath = @"C:\Users\Ildar\Downloads\Fedex\Fedex_Adjusted.xlsx";
            IList<OrderShippingInfoDTO> alreadyAdjShippings = new List<OrderShippingInfoDTO>();
            using (var stream = new FileStream(adjustedFilepath, FileMode.Open, FileAccess.ReadWrite))
            {
                IWorkbook workbook = null;
                if (adjustedFilepath.EndsWith(".xlsx"))
                    workbook = new XSSFWorkbook(stream);
                else
                    workbook = new HSSFWorkbook(stream);

                var sheet = workbook.GetSheetAt(0);

                for (var i = 1; i <= sheet.LastRowNum; i++)
                {
                    var row = sheet.GetRow(i);

                    if (row.GetCell(4) != null)
                    {
                        var trackingNumber = row.GetCell(4).ToString();
                        var adjAmount = StringHelper.TryGetDecimal(row.GetCell(12).ToString());

                        alreadyAdjShippings.Add(new OrderShippingInfoDTO()
                        {
                            TrackingNumber = trackingNumber,
                            StampsShippingCost = adjAmount,
                        });
                    }
                }
            }

            var filepath = @"C:\Users\Ildar\Downloads\Fedex\Oct20_Nov20_2018.xlsx";
            using (var db = _dbFactory.GetRWDb())
            {
                using (var stream = new FileStream(filepath, FileMode.Open, FileAccess.ReadWrite))
                {
                    IWorkbook workbook = null;
                    if (filepath.EndsWith(".xlsx"))
                        workbook = new XSSFWorkbook(stream);
                    else
                        workbook = new HSSFWorkbook(stream);

                    var sheet = workbook.GetSheetAt(0);

                    for (var i = 1; i <= sheet.LastRowNum; i++)
                    {
                        var row = sheet.GetRow(i);

                        var trackingNumber = row.GetCell(8).ToString();

                        var shipping = db.OrderShippingInfos.GetAll().FirstOrDefault(sh => sh.TrackingNumber == trackingNumber);

                        if (shipping != null)
                        {
                            row.GetCell(11).SetCellValue(shipping.StampsShippingCost.ToString());
                        }

                        var hasFix = alreadyAdjShippings.FirstOrDefault(sh => sh.TrackingNumber == trackingNumber);
                        if (hasFix != null)
                        {
                            row.GetCell(12).SetCellValue("Yes");
                            row.GetCell(13).SetCellValue(hasFix.StampsShippingCost.ToString());
                        }
                        else
                        {
                            row.GetCell(12).SetCellValue("No");
                        }
                    }

                    var filepathUpdated = Path.Combine(Path.GetDirectoryName(filepath),
                        Path.GetFileName(filepath) + "_updated" + Path.GetExtension(filepath));

                    using (var outputStream = new FileStream(filepathUpdated, FileMode.Create, FileAccess.ReadWrite))
                    {
                        workbook.Write(outputStream);
                    }
                }
            }
        }

        public void ResetAndCreateBatch(string filepath)
        {
            var trackingNumbers = File.ReadAllLines(filepath);
            _log.Info("Tracking numbers, source: " + trackingNumbers.Count());
            trackingNumbers = trackingNumbers.Where(tr => !String.IsNullOrEmpty(tr)).Distinct().ToArray();
            trackingNumbers.ForEach(tr => tr = StringHelper.TrimWhitespace(tr));

            _log.Info("Tracking numbers, final: " + trackingNumbers.Count());

            using (var db = _dbFactory.GetRWDb())
            {
                //var shippingInfoes = (from sh in db.OrderShippingInfos.GetAll()
                //                     join o in db.Orders.GetAll() on sh.OrderId equals o.Id
                //                     where o.BatchId == 314
                //                        && sh.IsActive
                //                     select sh).ToList();
                var shippingInfoes = db.OrderShippingInfos.GetAll().Where(sh => trackingNumbers.Any(tr => tr.EndsWith(sh.TrackingNumber))).ToList();
                if (shippingInfoes.Count() != trackingNumbers.Count())
                {
                    var missingTrackings = trackingNumbers.Where(tr => !shippingInfoes.Any(sh => sh.TrackingNumber == tr)).ToList();
                    _log.Error("Not found all shippings, found: " + shippingInfoes.Count() + ", missing: " + String.Join(", ", missingTrackings));
                    return;
                }

                var orderIds = shippingInfoes.Select(sh => sh.OrderId).ToList();
                var existShippingIds = shippingInfoes.Select(sh => sh.Id).ToList();
                var missingMultiPackageOrderShippings = db.OrderShippingInfos.GetAll().Where(sh => orderIds.Contains(sh.OrderId)
                    && sh.IsActive
                    && !existShippingIds.Contains(sh.Id));
                if (missingMultiPackageOrderShippings.Any())
                {
                    _log.Error("Missing part packages for orders: " + String.Join(", ", missingMultiPackageOrderShippings.Select(sh => sh.OrderId + ": " + sh.TrackingNumber)));
                    return;
                }

                var newBatch = new OrderBatch()
                {
                    Name = "Reprint: " + Path.GetFileName(filepath),
                    CreateDate = _time.GetAppNowTime(),
                    Type = (int)BatchType.User,
                };
                db.OrderBatches.Add(newBatch);
                db.Commit();

                var orders = db.Orders.GetAll().Where(o => orderIds.Contains(o.Id)).ToList();
                orders.ForEach(o =>
                {
                    o.BatchId = newBatch.Id;
                    o.ShipmentProviderType = (int)ShipmentProviderType.FedexGeneral;
                });
                db.Commit();

                foreach (var sh in shippingInfoes)
                {
                    var item = trackingNumbers.FirstOrDefault(tr => tr.EndsWith(sh.TrackingNumber));
                    var index = Array.IndexOf(trackingNumbers, item);

                    sh.CustomLabelSortOrder = index;
                    sh.LabelPath = null;

                    sh.LabelPurchaseDate = null;
                    sh.LabelPurchaseBy = null;
                    sh.LabelPurchaseMessage = null;
                    sh.LabelPurchaseResult = null;

                    sh.TrackingNumber = null;
                    sh.TrackingRequestAttempts = 0;
                    sh.TrackingStateDate = null;
                    sh.TrackingStateEvent = null;
                    sh.TrackingStateSource = null;
                    sh.IsDelivered = false;
                    sh.IsFeedSubmitted = false;
                    sh.IsFulfilled = false;

                    sh.ShippingDate = null;
                    sh.ShipmentProviderType = (int)ShipmentProviderType.FedexGeneral;
                    if (sh.ShippingMethodId == ShippingUtils.FedexOneRate2DayEnvelope)
                    {
                        sh.ShippingMethodId = ShippingUtils.FedexPriorityOvernightEnvelope;
                        sh.StampsShippingCost = 40;
                    }
                    if (sh.ShippingMethodId == ShippingUtils.FedexOneRate2DayPak)
                    {
                        sh.ShippingMethodId = ShippingUtils.FedexPriorityOvernightPak;
                        sh.StampsShippingCost = 60;
                    }
                }
                db.Commit();
            }
        }

        public void TestGetSmartPostRates(string orderId)
        {
            var weightService = new WeightService();
            var companyAddress = new CompanyAddressService(_company);

            using (var db = _dbFactory.GetRWDb())
            {
                var order = db.ItemOrderMappings.GetOrderWithItems(weightService, orderId, unmaskReferenceStyle: false, includeSourceItems: true);
                var shippingService = ShippingUtils.InitialShippingServiceIncludeUpgrade(order.InitialServiceType, order.UpgradeLevel); //order.ShippingService
                var orderItemInfoes = OrderHelper.BuildAndGroupOrderItems(order.Items);
                var sourceOrderItemInfoes = OrderHelper.BuildAndGroupOrderItems(order.SourceItems);

                var serviceFactory = new ServiceFactory();

                var rateProviders = serviceFactory.GetShipmentProviders(_log,
                    _time,
                    _dbFactory,
                    weightService,
                    _company.ShipmentProviderInfoList,
                    AppSettings.DefaultCustomType,
                    AppSettings.LabelDirectory,
                    AppSettings.LabelDirectory,
                    AppSettings.LabelDirectory);

                var fedexRateProvider = rateProviders.FirstOrDefault(r => r.Type == ShipmentProviderType.FedexSmartPost);

                var rates = fedexRateProvider.GetLocalRate(
                        companyAddress.GetReturnAddress(order.GetMarketId()),
                        companyAddress.GetPickupAddress(order.GetMarketId()),
                        order.GetAddressDto(),
                        _time.GetAppNowTime(),
                        order.WeightD,
                        null,
                        order.IsInsured ? order.TotalPrice : 0,
                        order.IsSignConfirmation,
                        new OrderRateInfo()
                        {
                            ShippingService = shippingService,
                            InitialServiceType = order.InitialServiceType,
                            OrderNumber = order.OrderId,
                            Items = orderItemInfoes,
                            SourceItems = sourceOrderItemInfoes,
                            TotalPrice = order.TotalPrice,
                            Currency = order.TotalPriceCurrency,
                        },
                        RetryModeType.Normal);


                _log.Info("Rates: " + rates.Rates.Count);
            }
        }

        public void TestGetOneRateRates(string orderId)
        {
            var weightService = new WeightService();
            var companyAddress = new CompanyAddressService(_company);

            using (var db = _dbFactory.GetRWDb())
            {
                var order = db.ItemOrderMappings.GetOrderWithItems(weightService, orderId, unmaskReferenceStyle: false, includeSourceItems: true);
                var shippingService = ShippingUtils.InitialShippingServiceIncludeUpgrade(order.InitialServiceType, order.UpgradeLevel); //order.ShippingService
                var orderItemInfoes = OrderHelper.BuildAndGroupOrderItems(order.Items);
                var sourceOrderItemInfoes = OrderHelper.BuildAndGroupOrderItems(order.SourceItems);

                var serviceFactory = new ServiceFactory();

                var rateProviders = serviceFactory.GetShipmentProviders(_log,
                    _time,
                    _dbFactory,
                    weightService,
                    _company.ShipmentProviderInfoList,
                    AppSettings.DefaultCustomType,
                    AppSettings.LabelDirectory,
                    AppSettings.LabelDirectory,
                    AppSettings.LabelDirectory);

                var fedexRateProvider = rateProviders.FirstOrDefault(r => r.Type == ShipmentProviderType.FedexOneRate);

                var rates = fedexRateProvider.GetLocalRate(
                        companyAddress.GetReturnAddress(order.GetMarketId()),
                        companyAddress.GetPickupAddress(order.GetMarketId()),
                        order.GetAddressDto(),
                        _time.GetAppNowTime(),
                        order.WeightD,
                        null,
                        order.IsInsured ? order.TotalPrice : 0,
                        order.IsSignConfirmation,
                        new OrderRateInfo()
                        {
                            ShippingService = shippingService,
                            InitialServiceType = order.InitialServiceType,
                            OrderNumber = order.OrderId,
                            Items = orderItemInfoes,
                            SourceItems = sourceOrderItemInfoes,
                            TotalPrice = order.TotalPrice,
                            Currency = order.TotalPriceCurrency,
                        },
                        RetryModeType.Normal);


                _log.Info("Rates: " + rates.Rates.Count);
            }
        }
    }
}
