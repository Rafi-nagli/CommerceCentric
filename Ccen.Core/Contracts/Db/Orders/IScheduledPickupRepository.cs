using System;
using System.Collections.Generic;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Orders;
using Amazon.Core.Models;
using Amazon.Core.Models.Settings;
using Amazon.DTO;
using Amazon.DTO.Orders;

namespace Amazon.Core.Contracts.Db
{
    public interface IScheduledPickupRepository : IRepository<ScheduledPickup>
    {
        ScheduledPickupDTO GetLast(ShipmentProviderType providerType);
        int Store(ScheduledPickupDTO pickupResult,
            DateTime when,
            long? by);
    }
}
