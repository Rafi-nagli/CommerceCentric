using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Common.Emails;
using Amazon.Common.Helpers;
using Amazon.Common.Threads;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Stamps;
using Amazon.Core.Models;
using Amazon.Core.Models.Settings;
using Amazon.DAL;
using Amazon.DTO;
using Amazon.DTO.Users;
using Amazon.Model.General;
using Amazon.Model.Implementation;
using Amazon.Model.Implementation.Trackings.Rules;
using Ccen.Model.SyncService;

namespace Amazon.Model.SyncService.Threads.Simple
{
    public class UpdateOrderTrackingStatus : ThreadBase
    {
        public UpdateOrderTrackingStatus(long companyId, ISystemMessageService messageService, TimeSpan? callbackInterval = null)
            : base("UpdateOrderTrackingStatus", companyId, messageService, callbackInterval)
        {

        }

        protected override void RunCallback()
        {
            var dbFactory = new DbFactory();
            CompanyDTO company;
            using (var db = dbFactory.GetRDb())
            {
                company = db.Companies.GetByIdWithSettingsAsDto(CompanyId);
            }

            var time = new TimeService(dbFactory);
            var log = GetLogger();
            var serviceFactory = new ServiceFactory();
            var actionService = new SystemActionService(GetLogger(), time);
            var notificationService = new NotificationService(GetLogger(), time, dbFactory);
            var companyAddress = new CompanyAddressService(company);
            var addressService = new AddressService(null, companyAddress.GetReturnAddress(MarketIdentifier.Empty()), companyAddress.GetPickupAddress(MarketIdentifier.Empty()));

            var emailService = new EmailService(GetLogger(), 
                SettingsBuilder.GetSmtpSettingsFromCompany(company, AppSettings.IsDebug, AppSettings.IsSampleLabels),
                addressService);
                
                
            var ruleList = new List<ITrackingRule>()
            {
                new NeverShippedTrackingRule(GetLogger(), notificationService, time),
                new GetStuckTrackingRule(GetLogger(), notificationService, time),
                new NoticeLeftTrackingRule(GetLogger(), actionService, addressService, time),
                new UndeliverableAsAddressedTrackingRule(GetLogger(), actionService, addressService, emailService, time)
            };

            var trackingService = new TrackingManager(GetLogger(),
                actionService,
                addressService,
                emailService,
                time,
                ruleList);

            var trackingProviderTypes = new List<ShipmentProviderType>()
            {
                ShipmentProviderType.Stamps,
                ShipmentProviderType.Dhl,
                //ShipmentProviderType.DhlECom,
                ShipmentProviderType.FedexOneRate,
            };

            using (var db = dbFactory.GetRWDb())
            {
                foreach (var trackingProviderType in trackingProviderTypes)
                {
                    try
                    {
                        var trackingProvider = serviceFactory.GetTrackingProviderByType(trackingProviderType,
                            company,
                            company.ShipmentProviderInfoList,
                            dbFactory,
                            log,
                            time);

                        if (trackingProvider == null)
                            continue;

                        LogWrite("UpdateAllShippedOrderStatus " + trackingProviderType);
                        UpdateAllShippedOrderStatus(trackingService,
                            time,
                            db,
                            trackingProvider,
                            company);
                    }
                    catch (Exception ex)
                    {
                        LogError("TrackingProviderType: " + trackingProviderType, ex);
                    }
                }
            }
        }

        public void UpdateAllShippedOrderStatus(TrackingManager trackingService, 
            ITime time, 
            IUnitOfWork db, 
            ITrackingProvider trackingProvider,
            CompanyDTO company)
        {
            var carrierNames = ShippingServiceUtils.GetRelatedCarrierNames(trackingProvider.Carrier);
            
            var shippings = db.Orders.GetUnDeliveredMailInfoes(time.GetUtcTime(), true, null)
                .OrderBy(o => o.OrderDate) //NOTE: first reprocess old trackings
                .Where(sh => carrierNames.Contains(sh.Carrier))
                .Take(500)
                .ToList();

            shippings.AddRange(db.Orders.GetUnDeliveredShippingInfoes(time.GetUtcTime(), true, null)
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
