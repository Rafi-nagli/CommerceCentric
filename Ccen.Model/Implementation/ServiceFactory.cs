using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Amazon.Api;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Contracts.Stamps;
using Amazon.Core.Models;
using Amazon.Core.Models.Settings;
using Amazon.DTO;
using Amazon.DTO.Contracts;
using Amazon.DTO.Users;
using Amazon.Model.Implementation.Addresses;
using Amazon.Model.Implementation.Trackings;
using Dhl.Api;
using DhlECommerce.Api;
using DhlECommerce.Api.Models;
using Fedex.Api;
using IBC.Api;
using ShipFIMS.Api;
using SkyPostal.Api;
using Stamps.Api;

namespace Amazon.Model.Implementation
{
    public class ServiceFactory : IServiceFactory
    {
        public ITrackingProvider GetTrackingProviderByType(
            ShipmentProviderType providerType,
            CompanyDTO company,
            IList<ShipmentProviderDTO> shipmentProviderInfos,
            IDbFactory dbFactory,
            ILogService log,
            ITime time)
        {
            var providerInfo = company.ShipmentProviderInfoList.FirstOrDefault(sh => sh.Type == (int)providerType);
            if (providerInfo == null)
                return null;
                //throw new NotSupportedException("Tracking providerType=" + providerType + " - not supported");

            switch (providerType)
            {
                case ShipmentProviderType.Dhl:
                    return new DhlTrackingProvider(log,
                        time,
                        providerInfo.EndPointUrl,
                        providerInfo.UserName,
                        providerInfo.Password,
                        providerInfo.Key1);

                case ShipmentProviderType.Stamps:
                    //return new ComposedUspsAndCanadaPostTrackingProvider(log,
                    //    time,
                    //    company.USPSUserId,
                    //    company.CanadaPostKeys);
                    return new UspsTrackingProvider(log, time, company.USPSUserId);

                case ShipmentProviderType.DhlECom:
                    return new DhlECommerceTrackingProvider(log,
                        time,
                        providerInfo.UserName,
                        providerInfo.Password,
                        providerInfo.Key1,
                        providerInfo.Key2,
                        providerInfo.Key3,
                        GetFreshTokenInfo(dbFactory, ShipmentProviderType.DhlECom)?.Token,
                        () => GetFreshTokenInfo(dbFactory, ShipmentProviderType.DhlECom),
                            (t) => StoreNewTokenInfo(dbFactory, ShipmentProviderType.DhlECom, t));

                case ShipmentProviderType.FedexGeneral:
                case ShipmentProviderType.FedexOneRate:
                case ShipmentProviderType.FedexSmartPost:
                    return new FedexTrackingApi(log,
                        time,
                        providerInfo.EndPointUrl,
                        providerInfo.UserName,
                        providerInfo.Password,
                        providerInfo.Key1,
                        providerInfo.Key2,
                        providerInfo.Key3,
                        company.ShortName);
            }

            throw new NotSupportedException("No implementation for trackingProvider=" + providerType);
        }

        public IShipmentApi GetShipmentProviderByType(
            ShipmentProviderType providerType,
            ILogService log,
            ITime time,
            IDbFactory dbFactory,
            IWeightService weightService,
            IList<ShipmentProviderDTO> shipmentProviderInfos,
            string defaultCustomType,
            string outputDirectory,
            string reserveDirectory,
            string templateDirectory)
        {
            var trackingNumberService = new TrackingNumberService(log, time, dbFactory);
            var supportedShippingMethods = new List<ShippingMethodDTO>();

            using (var db = dbFactory.GetRDb())
            {
                supportedShippingMethods = db.ShippingMethods
                    .GetAllAsDto()
                    .Where(m => m.ShipmentProviderType == (int)providerType
                        && m.IsActive)
                    .ToList();
            }

            switch (providerType)
            {
                case ShipmentProviderType.Stamps:
                    var stampsProvider =
                        shipmentProviderInfos.FirstOrDefault(p => p.Type == (int) ShipmentProviderType.Stamps);
                    if (stampsProvider != null)
                    {
                        var authList =
                            shipmentProviderInfos.Where(p => p.Type == (int) ShipmentProviderType.Stamps ||
                                                             p.Type == (int) ShipmentProviderType.StampsPriority)
                                .ToList();
                        return new StampsShipmentApi(stampsProvider.Id,
                            (ShipmentProviderType)stampsProvider.Type,
                            log,
                            time,
                            weightService,
                            trackingNumberService,
                            authList.ToList<IStampsAuthInfo>(),               
                            LabelFormat.Jpg,
                            false,
                            "Children Clothes",
                            true,
                            outputDirectory,
                            reserveDirectory,
                            false);
                    }
                    return null;

                case ShipmentProviderType.StampsPriority:
                    var stampsPriorityProvider =
                        shipmentProviderInfos.FirstOrDefault(p => p.Type == (int)ShipmentProviderType.StampsPriority);
                    if (stampsPriorityProvider != null)
                    {
                        var authList =
                            shipmentProviderInfos.Where(p => p.Type == (int)ShipmentProviderType.Stamps ||
                                                             p.Type == (int)ShipmentProviderType.StampsPriority)
                                .ToList();
                        return new StampsShipmentApi(stampsPriorityProvider.Id,
                            (ShipmentProviderType)stampsPriorityProvider.Type,
                            log,
                            time,
                            weightService,
                            trackingNumberService,
                            authList.ToList<IStampsAuthInfo>(),
                            LabelFormat.Jpg,
                            false,
                            "Children Clothes",
                            true,
                            outputDirectory,
                            reserveDirectory,
                            false);
                    }
                    return null;

                case ShipmentProviderType.Amazon:
                    var amazonProvider =
                        shipmentProviderInfos.FirstOrDefault(p => p.Type == (int) ShipmentProviderType.Amazon);
                    if (amazonProvider != null)
                    {
                        return new AmazonShipmentApi(amazonProvider.Id,
                            log,
                            time,
                            weightService,
                            supportedShippingMethods,
                            amazonProvider.Key1,
                            amazonProvider.Key2,
                            amazonProvider.Password,
                            amazonProvider.StampUsername,
                            amazonProvider.EndPointUrl,
                            outputDirectory,
                            reserveDirectory,
                            templateDirectory);
                    }
                    return null;
                
                case ShipmentProviderType.Dhl:
                    var dhlProvider =
                        shipmentProviderInfos.FirstOrDefault(p => p.Type == (int) ShipmentProviderType.Dhl);
                    if (dhlProvider != null)
                    {
                        return new DhlShipmentApi(dhlProvider.Id,
                            log,
                            time,
                            dhlProvider.EndPointUrl,
                            dhlProvider.UserName,
                            dhlProvider.Password,
                            dhlProvider.Key1,
                            dhlProvider.Key2,
                            outputDirectory,
                            reserveDirectory,
                            templateDirectory);
                    }
                    return null;

                case ShipmentProviderType.DhlECom:
                    var dhlEComProvider =
                        shipmentProviderInfos.FirstOrDefault(p => p.Type == (int)ShipmentProviderType.DhlECom);
                    if (dhlEComProvider != null)
                    {
                        return new DhlECommerceShipmentApi(dhlEComProvider.Id,
                            log,
                            time,
                            dbFactory,
                            weightService,
                            dhlEComProvider.UserName,
                            dhlEComProvider.Password,
                            dhlEComProvider.Key1,
                            dhlEComProvider.Key2,
                            dhlEComProvider.Key3,
                            defaultCustomType,
                            outputDirectory,
                            reserveDirectory,
                            GetFreshTokenInfo(dbFactory, ShipmentProviderType.DhlECom)?.Token,
                            () => GetFreshTokenInfo(dbFactory, ShipmentProviderType.DhlECom),
                            (t) => StoreNewTokenInfo(dbFactory, ShipmentProviderType.DhlECom, t));
                    }
                    return null;

                case ShipmentProviderType.IBC:
                    var ibcProvider =
                        shipmentProviderInfos.FirstOrDefault(p => p.Type == (int)ShipmentProviderType.IBC);
                    if (ibcProvider != null)
                    {
                        return new IBCShipmentApi(ibcProvider.Id,
                            log,
                            time,
                            dbFactory,
                            weightService,
                            PortalEnum.PA,
                            ibcProvider.EndPointUrl,
                            ibcProvider.UserName,
                            ibcProvider.Password,
                            ibcProvider.Key1,
                            defaultCustomType,
                            outputDirectory,
                            reserveDirectory);
                    }
                    return null;

                case ShipmentProviderType.SkyPostal:
                    var skyPostalProvider =
                        shipmentProviderInfos.FirstOrDefault(p => p.Type == (int)ShipmentProviderType.SkyPostal);
                    if (skyPostalProvider != null)
                    {
                        return new SkyPostalShipmentApi(skyPostalProvider.Id,
                            log,
                            time,
                            dbFactory,
                            weightService,
                            PortalEnum.PA,
                            skyPostalProvider.EndPointUrl,
                            skyPostalProvider.UserName,
                            skyPostalProvider.Password,
                            skyPostalProvider.Key1,
                            skyPostalProvider.Key2,
                            skyPostalProvider.Key3,
                            defaultCustomType,
                            outputDirectory,
                            reserveDirectory);
                    }
                    return null;

                case ShipmentProviderType.FedexSmartPost:
                case ShipmentProviderType.FedexOneRate:
                case ShipmentProviderType.FedexGeneral:
                    var fedexProvider =
                        shipmentProviderInfos.FirstOrDefault(p => p.Type == (int)providerType);
                    if (fedexProvider != null)
                    {
                        return new FedexShipmentApi(PortalEnum.PA,
                            fedexProvider.Id,
                            log,
                            time,               
                            weightService,
                            fedexProvider.EndPointUrl,
                            fedexProvider.UserName,
                            fedexProvider.Password,
                            fedexProvider.Key1,
                            fedexProvider.Key2,
                            fedexProvider.Key3,
                            (FedexRateTypes)Int32.Parse(fedexProvider.Key4),
                            defaultCustomType,
                            outputDirectory,
                            reserveDirectory,
                            templateDirectory);
                    }
                    return null;
                case ShipmentProviderType.FIMS:
                    var fimsProvider =
                        shipmentProviderInfos.FirstOrDefault(p => p.Type == (int)providerType);
                    if (fimsProvider != null)
                    {
                        return new FIMSShipmentApi(fimsProvider.Id,
                            log,
                            time,
                            fimsProvider.EndPointUrl,
                            fimsProvider.Key1,
                            fimsProvider.Key2,
                            fimsProvider.Key3, //air bill number
                            defaultCustomType,
                            outputDirectory,
                            reserveDirectory,
                            templateDirectory);
                    }
                    return null;

            }
            return null;
        }

        private TokenInfo GetFreshTokenInfo(IDbFactory dbFactory, ShipmentProviderType type)
        {
            using (var db = dbFactory.GetRWDb())
            {
                var marketInfo = db.ShipmentProviders.GetAllAsDto().FirstOrDefault(m => m.Type == (int)type);
                if (marketInfo != null)
                {
                    return new TokenInfo()
                    {
                        Token = marketInfo.Key4
                    };
                }
            }
            return null;
        }

        private void StoreNewTokenInfo(IDbFactory dbFactory, ShipmentProviderType type, TokenInfo token)
        {
            using (var db = dbFactory.GetRWDb())
            {
                var marketInfo = db.ShipmentProviders.GetAll().FirstOrDefault(m => m.Type == (int)type);
                if (marketInfo != null)
                {
                    marketInfo.Key4 = token.Token;
                    db.Commit();
                }
            }
        }

        public IList<IShipmentApi> GetShipmentProviders(ILogService log,
            ITime time,
            IDbFactory dbFactory,
            IWeightService weightService,
            IList<ShipmentProviderDTO> shipmentProviderInfos,
            string defaultCustomType,
            string outputDirectory,
            string reserveDirectory,
            string templateDirectory)
        {
            var results = new List<IShipmentApi>();
            var providerTypeList = new List<ShipmentProviderType>()
            {
                ShipmentProviderType.Stamps,
                ShipmentProviderType.StampsPriority,
                ShipmentProviderType.Amazon,
                ShipmentProviderType.Dhl,
                ShipmentProviderType.DhlECom,
                ShipmentProviderType.IBC,
                ShipmentProviderType.SkyPostal,
                ShipmentProviderType.FIMS,
                ShipmentProviderType.FedexSmartPost,
                ShipmentProviderType.FedexOneRate,
                ShipmentProviderType.FedexGeneral                
            };

            foreach (var providerType in providerTypeList)
            {
                IShipmentApi provider = null;
                provider = GetShipmentProviderByType(providerType,
                    log,
                    time,
                    dbFactory,
                    weightService,
                    shipmentProviderInfos,
                    defaultCustomType,
                    outputDirectory,
                    reserveDirectory,
                    templateDirectory);

                if (provider != null)
                    results.Add(provider);
            }
            
            return results;
        }

        public IList<IAddressCheckService> GetAddressCheckServices(ILogService log,
            ITime time,
            IDbFactory dbFactory,
            IList<AddressProviderDTO> addressProviders)
        {
            var stampsProvider = addressProviders.FirstOrDefault(p => p.Type == (int) AddressProviderType.Stamps);
            var googleProvider = addressProviders.FirstOrDefault(p => p.Type == (int)AddressProviderType.Google);
            var mellisaProvider = addressProviders.FirstOrDefault(p => p.Type == (int)AddressProviderType.Mellisa);
            var selfCorrectionProvider = addressProviders.FirstOrDefault(p => p.Type == (int)AddressProviderType.SelfCorrection);
            var fedexProvider = addressProviders.FirstOrDefault(p => p.Type == (int)AddressProviderType.Fedex);

            var results = new List<IAddressCheckService>();
            if (stampsProvider != null)
                results.Add(new StampsAddressCheckService(log, stampsProvider));

            if (selfCorrectionProvider != null)
                results.Add(new PreviousCorrectionAddressCheckService(dbFactory));

            if (googleProvider != null)
                results.Add(new GoogleGeocodeAddressCheckService(log, googleProvider));

            if (fedexProvider != null)
                results.Add(new FedexAddressCheckService(log, time, fedexProvider, null));


            return results;
        }
    }
}
