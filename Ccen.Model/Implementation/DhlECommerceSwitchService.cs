using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models;
using Amazon.Core.Models.Search;
using Amazon.Core.Models.Settings;
using Amazon.DTO;
using Amazon.DTO.Users;
using Amazon.Model.General;
using Amazon.Model.Implementation.Errors;
using Amazon.Model.Implementation.Markets.Amazon;

namespace Amazon.Model.Implementation
{
    public class DhlECommerceSwitchService
    {
        private ILogService _log;
        private ITime _time;
        private CompanyDTO _company;
        private IDbFactory _dbFactory;
        private IEmailService _emailService;
        private IWeightService _weightService;
        private ISystemMessageService _messageService;

        public DhlECommerceSwitchService(ILogService log,
            ITime time,
            CompanyDTO company,
            IDbFactory dbFactory,
            IEmailService emailService,
            IWeightService weightService,
            ISystemMessageService messageService)
        {
            _log = log;
            _time = time;
            _company = company;
            _dbFactory = dbFactory;
            _emailService = emailService;
            _weightService = weightService;
            _messageService = messageService;
        }

        public void SwitchToECommerce()
        {
            var syncInfo = new EmptySyncInformer(_log, SyncType.Orders);
            var serviceFactory = new ServiceFactory();
            var rateProviders = serviceFactory.GetShipmentProviders(_log,
                    _time,
                    _dbFactory,
                    _weightService,
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
                        _weightService,
                        _messageService);

            var toSwitchNumber = 100;
            using (var db = _dbFactory.GetRWDb())
            {
                //Caclulate correction number
                var correctionNumber = 0;
                var fromDate = DateHelper.GetStartOfWeek(_time.GetAppNowTime());
                var toDate = _time.GetAppNowTime().Date;
                var shippingQuery = from o in db.Orders.GetAll()
                    join sh in db.OrderShippingInfos.GetAllAsDto() on o.Id equals sh.OrderId
                    where sh.ShipmentProviderType == (int) ShipmentProviderType.DhlECom
                          && o.OrderStatus == OrderStatusEnumEx.Shipped
                          && sh.LabelPurchaseDate >= fromDate
                          && sh.LabelPurchaseDate <= toDate
                          && sh.IsActive
                    select sh.Id;

                var shippedCount = shippingQuery.Count();
                var workDayCount = _time.GetBizDaysCount(fromDate, _time.GetAppNowTime());
                correctionNumber = workDayCount * 100 - shippedCount;
                if (correctionNumber > 100)
                    correctionNumber = 0;

                toSwitchNumber += correctionNumber;
                _log.Info("To switch number=" + correctionNumber);

                IList<DTOOrder> dtoOrders = db.ItemOrderMappings.GetFilteredOrdersWithItems(_weightService,
                    new OrderSearchFilter()
                    {
                        OrderStatus = OrderStatusEnumEx.AllUnshipped
                    }).ToList();// orderIds.ToArray(), includeSourceItems: true).ToList();

                //STEP 1. Switch all stamps firstclass orders
                var switchedCount = (from o in db.Orders.GetAll()
                                    join sh in db.OrderShippingInfos.GetAllAsDto() on o.Id equals sh.OrderId
                                    where sh.ShipmentProviderType == (int)ShipmentProviderType.DhlECom
                                          && o.OrderStatus == OrderStatusEnumEx.Unshipped
                                          && sh.IsActive
                                    select sh.Id).Count();

                foreach (var dtoOrder in dtoOrders)
                {
                    if (dtoOrder.ShipmentProviderType == (int) ShipmentProviderType.Stamps
                        && dtoOrder.ShippingInfos.Where(sh => sh.IsActive)
                            .All(sh => sh.ShippingMethodId == ShippingUtils.FirstClassShippingMethodId))
                    {
                        _log.Info("Change provider for order: " + dtoOrder.OrderId + ", from=" + dtoOrder.ShipmentProviderType);
                        var order = db.Orders.GetById(dtoOrder.Id);
                        order.ShipmentProviderType = (int)ShipmentProviderType.DhlECom;
                        db.Commit();
                        dtoOrder.ShipmentProviderType = (int)ShipmentProviderType.DhlECom;

                        if (synchronizer.UIUpdate(db, dtoOrder, false, false, false, switchToMethodId: null))
                        {
                            switchedCount++;
                            _log.Info("Success");
                        }
                        else
                        {
                            _log.Info("Failed");
                        }
                    }
                }

                var amazonOrderToSwitch = toSwitchNumber - switchedCount;
                foreach (var dtoOrder in dtoOrders)
                {
                    _log.Info("Change provider for order: " + dtoOrder.OrderId + ", from=" + dtoOrder.ShipmentProviderType);

                    if (amazonOrderToSwitch > 0)
                    {
                        if (dtoOrder.ShipmentProviderType == (int) ShipmentProviderType.Amazon
                            && dtoOrder.ShippingInfos.Where(sh => sh.IsActive)
                            .All(sh => sh.ShippingMethodId == ShippingUtils.AmazonFirstClassShippingMethodId))
                        {
                            var order = db.Orders.GetById(dtoOrder.Id);
                            order.ShipmentProviderType = (int) ShipmentProviderType.DhlECom;
                            db.Commit();
                            dtoOrder.ShipmentProviderType = (int) ShipmentProviderType.DhlECom;

                            if (synchronizer.UIUpdate(db, dtoOrder, false, false, false, null))
                            {
                                switchedCount++;
                                amazonOrderToSwitch--;
                                _log.Info("Success");
                            }
                            else
                            {
                                _log.Info("Failed");
                            }
                        }
                    }
                }

                _emailService.SendSystemEmailToAdmin("Dhl eCommerce, switched count: " + switchedCount, 
                    "Correction count: " + correctionNumber + ", should switched: " + toSwitchNumber);
            }
        }
    }
}
