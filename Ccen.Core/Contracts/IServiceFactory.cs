using System;
using System.Collections.Generic;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Models.Settings;
using Amazon.DTO.Users;

namespace Amazon.Core.Contracts
{
    public interface IServiceFactory
    {
        IList<IAddressCheckService> GetAddressCheckServices(ILogService log,
            ITime time,
            IDbFactory dbFactory,
            IList<AddressProviderDTO> addressProviders);


        IList<IShipmentApi> GetShipmentProviders(ILogService log,
            ITime time,
            IDbFactory dbFactory,
            IWeightService weightService,
            IList<ShipmentProviderDTO> shipmentProviderInfos,
            string defaultCustomType,
            string outputDirectory,
            string reserveDirectory,
            string templateDirectory);

        IShipmentApi GetShipmentProviderByType(
            ShipmentProviderType providerType,
            ILogService log,
            ITime time,
            IDbFactory dbFactory,
            IWeightService weightService,
            IList<ShipmentProviderDTO> shipmentProviderInfos,
            string defaultCustomType,
            string outputDirectory,
            string reserveDirectory,
            string templateDirectory);
    }
}
