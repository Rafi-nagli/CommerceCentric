using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Contracts.Notifications;
using Amazon.Core.Contracts.Stamps;
using Amazon.Core.Models;
using Amazon.Core.Models.Settings;
using Amazon.DAL;
using Amazon.DTO.Users;
using Amazon.Model.Implementation.Trackings.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Model.Implementation.Trackings
{
    public class TrackingProcessing
    {
        private ILogService _log;
        private ITime _time;
        private IDbFactory _dbFactory;
        private IEmailService _emailService;
        private INotificationService _notificationService;
        private ISystemActionService _actionService;
        private CompanyDTO _company;
        private IAddressService _addressService;
        private IServiceFactory _serviceFactory;

        public TrackingProcessing(ILogService log,
            ITime time,
            CompanyDTO company,
            IDbFactory dbFactory,
            IEmailService emailService,
            INotificationService notificationService,
            ISystemActionService actionService,
            IAddressService addressService,
            IServiceFactory serviceFactory)
        {
            _log = log;
            _time = time;
            _dbFactory = dbFactory;
            _emailService = emailService;
            _notificationService = notificationService;
            _actionService = actionService;
            _company = company;
            _addressService = addressService;
            _serviceFactory = serviceFactory;
        }


        public void ProcessForOrders(IList<long> orderIds)
        {
            Process(orderIds);
        }

        public void ProcessAll()
        {
            Process(null);
        }

        protected void Process(IList<long> orderIds)
        {
            var ruleList = new List<ITrackingRule>()
            {
                new NeverShippedTrackingRule(_log, _notificationService, _time),
                new GetStuckTrackingRule(_log, _notificationService, _time),
                new NoticeLeftTrackingRule(_log, _actionService, _addressService, _time),
                new UndeliverableAsAddressedTrackingRule(_log, _actionService, _addressService, _emailService, _time)
            };

            var trackingService = new TrackingManager(_log,
                _actionService,
                _addressService,
                _emailService,
                _time,
                ruleList);

            var trackingProviderTypes = new List<ShipmentProviderType>()
            {
                ShipmentProviderType.Stamps,
                ShipmentProviderType.Dhl,
                ShipmentProviderType.FedexOneRate
                //ShipmentProviderType.DhlECom
            };

            using (var db = _dbFactory.GetRWDb())
            {
                foreach (var trackingProviderType in trackingProviderTypes)
                {
                    try
                    {
                        var trackingProvider = (_serviceFactory as ServiceFactory).GetTrackingProviderByType(trackingProviderType,
                            _company,
                            _company.ShipmentProviderInfoList,
                            _dbFactory,
                            _log,
                            _time);

                        if (trackingProvider == null)
                            continue;

                        _log.Debug("UpdateAllShippedOrderStatus " + trackingProviderType);
                        UpdateAllShippedOrderStatus(trackingService,
                            _time,
                            db,
                            trackingProvider,
                            _company,
                            orderIds);
                    }
                    catch (Exception ex)
                    {
                        _log.Error("trackingProviderType: " + trackingProviderType, ex);
                    }
                }
            }
        }

        public void UpdateAllShippedOrderStatus(TrackingManager trackingService,
            ITime time,
            IUnitOfWork db,
            ITrackingProvider trackingProvider,
            CompanyDTO company,
            IList<long> orderIds)
        {
            var carrierNames = ShippingServiceUtils.GetRelatedCarrierNames(trackingProvider.Carrier);

            var shippings = db.Orders.GetUnDeliveredMailInfoes(time.GetUtcTime(), false, orderIds)
                .OrderBy(o => o.OrderDate) //NOTE: first reprocess old trackings
                .Where(sh => carrierNames.Contains(sh.Carrier))
                .Take(500)
                .ToList();

            shippings.AddRange(db.Orders.GetUnDeliveredShippingInfoes(time.GetUtcTime(), false, orderIds)
                .OrderBy(o => o.OrderDate) //NOTE: first reprocess old trackings
                .Where(sh => carrierNames.Contains(sh.Carrier))
                .Take(500)
                .ToList());

            trackingService.UpdateOrderTracking(db,
                company,
                shippings,
                trackingProvider);
        }
    }
}
