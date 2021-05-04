using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models;
using Amazon.Core.Models.Search;
using Amazon.Core.Models.Settings;
using Amazon.DTO;
using Amazon.DTO.Users;
using Amazon.Model.General;
using Amazon.Model.Implementation;
using Amazon.Model.Implementation.Errors;
using Amazon.Model.Implementation.Markets.Amazon;
using Amazon.Web.Models;

namespace Amazon.InventoryUpdateManual.CallActions
{
    public class CallOrderUpgradeProcessing
    {
        private ILogService _log;
        private ITime _time;
        private IDbFactory _dbFactory;
        private CompanyDTO _company;

        public CallOrderUpgradeProcessing(ILogService log,
            ITime time,
            IDbFactory dbFactory,
            CompanyDTO company)
        {
            _time = time;
            _log = log;
            _dbFactory = dbFactory;
            _company = company;
        }

        public void UpgradeOrders()
        {
            List<long> orderToUpgradeIdList = null;
            List<long> orderToResetUpgradeIdList = null;
            using (var db = _dbFactory.GetRWDb())
            {
                var orders = db.Orders.GetAll().Where(o => o.OrderStatus == OrderStatusEnumEx.Unshipped).ToList();
                var orderIdList = orders
                    .Where(o => !ShippingUtils.IsInternational(o.ShippingCountry)
                        && o.SourceShippingService == ShippingUtils.StandardServiceName)
                    .Select(o => o.Id)
                    .ToList();
                var shippings = db.OrderShippingInfos.GetAll().Where(sh => orderIdList.Contains(sh.OrderId)
                                                                           && sh.IsActive
                                                                           && sh.AvgDeliveryDays >= 1.99M).ToList();

                orderToUpgradeIdList = shippings.Select(sh => sh.OrderId).Distinct().ToList();
                
                var orderItems = db.OrderItemSources.GetAll().Where(oi => orderToUpgradeIdList.Contains(oi.OrderId)).ToList();
                orderToUpgradeIdList = orderToUpgradeIdList
                    .Where(o => orderItems.Where(oi => oi.OrderId == o).Sum(oi => oi.ItemPrice) > 10)
                    .ToList();
                
                var ordersToUpgrade = orders.Where(o => orderToUpgradeIdList.Contains(o.Id)).ToList();
                _log.Info(String.Join(", ", ordersToUpgrade.Select(o => o.AmazonIdentifier).ToList()));
                var zipList = ordersToUpgrade.Select(o => o.ShippingZip).ToList();
                _log.Info(String.Join(", ", zipList));
            }

            UpgradeOrderList(orderToUpgradeIdList);
            
        }

        public void UpgradeOrderList(IList<long> orderIds)
        {
            var syncInfo = new EmptySyncInformer(_log, SyncType.Orders);
            var serviceFactory = new ServiceFactory();
            var weightService = new WeightService();
            var messageService = new SystemMessageService(_log, _time, _dbFactory);

            var rateProviders = serviceFactory.GetShipmentProviders(_log,
                    _time,
                    _dbFactory,
                    weightService,
                    _company.ShipmentProviderInfoList,
                    null,
                    null,
                    null,
                    null);

            var companyAddress = new CompanyAddressService(_company);

            var synchronizer = new AmazonOrdersSynchronizer(_log,
                        _company,
                        syncInfo,
                        rateProviders,
                        companyAddress,
                        _time,
                        weightService,
                        messageService);

            using (var db = _dbFactory.GetRWDb())
            {
                IList<DTOOrder> dtoOrders = db.ItemOrderMappings.GetSelectedOrdersWithItems(weightService, orderIds.ToArray(), includeSourceItems: true).ToList();
                foreach (var dtoOrder in dtoOrders)
                {
                    _log.Info("Upgrade order: " + dtoOrder.OrderId);
                    //Update into DB, after success update
                    var order = db.Orders.GetById(dtoOrder.Id);
                    order.UpgradeLevel = 1;
                    db.Commit();
                    dtoOrder.UpgradeLevel = 1;

                    if (synchronizer.UIUpdate(db, dtoOrder, false, false, false, null))
                    {
                        _log.Info("Success");
                    }
                    else
                    {
                        _log.Info("Failed");
                    }
                }
            }
        }



        
        public void UpdateUnshippedShippingAvgDeliveryDays()
        {
            using (var db = _dbFactory.GetRWDb())
            {
                //var orders = db.Orders.GetAll().Where(o => o.BatchId == 1343).ToList();
                var orders = db.Orders.GetAll().Where(o => o.OrderStatus == OrderStatusEnumEx.Unshipped
                    && o.ShippingCountry == "US").ToList();

                var allZipCodesAsc = db.ZipCodes.GetAll().ToList().OrderBy(z => z.Zip).ToList();
                var allZipCodesDesc = allZipCodesAsc.OrderByDescending(z => z.Zip).ToList();

                foreach (var order in orders)
                {
                    var zip = order.ShippingZip;
                    _log.Info("Zip: " + zip);

                    ZipCode closestFirstClassZipCode = allZipCodesAsc.FirstOrDefault(z => z.Zip == zip && z.AverageFirstClassDeliveryDays.HasValue);
                    if (closestFirstClassZipCode == null)
                    {
                        var closestAsc = allZipCodesAsc.FirstOrDefault(z => String.Compare(z.Zip, zip) > 0 && z.AverageFirstClassDeliveryDays.HasValue);
                        var closestDesc = allZipCodesDesc.FirstOrDefault(z => String.Compare(z.Zip, zip) < 0 && z.AverageFirstClassDeliveryDays.HasValue);
                        if (closestAsc == null)
                            closestFirstClassZipCode = closestDesc;
                        if (closestDesc == null)
                            closestFirstClassZipCode = closestAsc;
                        if (closestAsc != null && closestDesc != null)
                        {
                            if (Math.Abs((StringHelper.TryGetInt(closestDesc.Zip) ?? 0) - (StringHelper.TryGetInt(zip) ?? 0))
                                > Math.Abs((StringHelper.TryGetInt(closestAsc.Zip) ?? 0) - (StringHelper.TryGetInt(zip) ?? 0)))
                                closestFirstClassZipCode = closestAsc;
                            else
                                closestFirstClassZipCode = closestDesc;
                        }
                    }

                    //ZipCode closestFlatZipCode = allZipCodesAsc.FirstOrDefault(z => z.Zip == zip && z.AverageFlatDeliveryDays.HasValue);
                    //if (closestFlatZipCode == null)
                    //{
                    //    var closestAsc = allZipCodesAsc.FirstOrDefault(z => String.Compare(z.Zip, zip) > 0 && z.AverageFlatDeliveryDays.HasValue);
                    //    var closestDesc = allZipCodesDesc.FirstOrDefault(z => String.Compare(z.Zip, zip) < 0 && z.AverageFlatDeliveryDays.HasValue);
                    //    if (closestAsc == null)
                    //        closestFlatZipCode = closestDesc;
                    //    if (closestDesc == null)
                    //        closestFlatZipCode = closestAsc;
                    //    if (closestAsc != null && closestDesc != null)
                    //    {
                    //        if (Math.Abs(StringHelper.TryGetInt(closestDesc.Zip) ?? 0 - StringHelper.TryGetInt(zip) ?? 0)
                    //            >
                    //            Math.Abs(StringHelper.TryGetInt(closestAsc.Zip) ?? 0 - StringHelper.TryGetInt(zip) ?? 0))
                    //            closestFlatZipCode = closestAsc;
                    //        else
                    //            closestFlatZipCode = closestDesc;
                    //    }
                    //}

                    //ZipCode closestRegularZipCode = allZipCodesAsc.FirstOrDefault(z => z.Zip == zip && z.AverageRegularDeliveryDays.HasValue);
                    //if (closestRegularZipCode == null)
                    //{
                    //    var closestAsc = allZipCodesAsc.FirstOrDefault(z => String.Compare(z.Zip, zip) > 0 && z.AverageRegularDeliveryDays.HasValue);
                    //    var closestDesc = allZipCodesDesc.FirstOrDefault(z => String.Compare(z.Zip, zip) < 0 && z.AverageRegularDeliveryDays.HasValue);
                    //    if (closestAsc == null)
                    //        closestRegularZipCode = closestDesc;
                    //    if (closestDesc == null)
                    //        closestRegularZipCode = closestAsc;
                    //    if (closestAsc != null && closestDesc != null)
                    //    {
                    //        if (Math.Abs(StringHelper.TryGetInt(closestDesc.Zip) ?? 0 - StringHelper.TryGetInt(zip) ?? 0)
                    //            >
                    //            Math.Abs(StringHelper.TryGetInt(closestAsc.Zip) ?? 0 - StringHelper.TryGetInt(zip) ?? 0))
                    //            closestRegularZipCode = closestAsc;
                    //        else
                    //            closestRegularZipCode = closestDesc;
                    //    }
                    //}
                    
                    var shippings = db.OrderShippingInfos.GetByOrderId(order.Id);
                    foreach (var shipping in shippings)
                    {
                        if (shipping.AvgDeliveryDays.HasValue)
                            continue;

                        if (closestFirstClassZipCode != null)
                        {
                            if (shipping.ShippingMethodId == 1 || shipping.ShippingMethodId == 15)
                            {
                                shipping.AvgDeliveryDaysByZip = closestFirstClassZipCode.Zip;
                                shipping.AvgDeliveryDays = closestFirstClassZipCode.AverageFirstClassDeliveryDays;
                            }
                        }
                        //if (closestFlatZipCode != null)
                        //{
                        //    if (shipping.ShippingMethodId == 2 || shipping.ShippingMethodId == 16)
                        //    {
                        //        shipping.AvgDeliveryDaysByZip = closestFlatZipCode.Zip;
                        //        shipping.AvgDeliveryDays = closestFlatZipCode.AverageFlatDeliveryDays;
                        //    }
                        //}
                        //if (closestRegularZipCode != null)
                        //{
                        //    if (shipping.ShippingMethodId == 3 || shipping.ShippingMethodId == 17)
                        //    {
                        //        shipping.AvgDeliveryDaysByZip = closestRegularZipCode.Zip;
                        //        shipping.AvgDeliveryDays = closestRegularZipCode.AverageRegularDeliveryDays;
                        //    }
                        //}
                    }
                    db.Commit();
                }
            }
        }
    }
}
