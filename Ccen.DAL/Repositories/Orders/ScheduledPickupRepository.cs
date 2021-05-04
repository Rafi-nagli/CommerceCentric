using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Views;
using Amazon.Core;
using Amazon.Core.Entities.Orders;
using Amazon.Core.Models;
using Amazon.Core.Models.Settings;
using Amazon.DTO;
using Amazon.DTO.Orders;

namespace Amazon.DAL.Repositories
{
    public class ScheduledPickupRepository : Repository<ScheduledPickup>, IScheduledPickupRepository
    {
        public ScheduledPickupRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IQueryable<ScheduledPickupDTO> GetAllAsDto()
        {
            return AsDto(GetAll());
        }

        public ScheduledPickupDTO GetAsDto(long id)
        {
            return AsDto(GetAll().Where(b => b.Id == id)).FirstOrDefault();
        }

        private IQueryable<ScheduledPickupDTO> AsDto(IQueryable<ScheduledPickup> query)
        {
            return query.Select(b => new ScheduledPickupDTO()
            {
                Id = b.Id,

                ProviderType = b.ProviderType,
                RequestPickupDate = b.RequestPickupDate,
                RequestReadyByTime = b.RequestReadyByTime,
                RequestCloseTime = b.RequestCloseTime,
                ResultMessage = b.ResultMessage,
                SendRequestDate = b.SendRequestDate,

                ConfirmationNumber = b.ConfirmationNumber,
                PickupDate = b.PickupDate,
                ReadyByTime = b.ReadyByTime,
                PickupCharge = b.PickupCharge,
                CallInTime = b.CallInTime,

                UpdateDate = b.UpdateDate,
                CreateDate = b.CreateDate,
            });
        }


        public ScheduledPickupDTO GetLast(ShipmentProviderType providerType)
        {
            return GetAllAsDto()
                .Where(p => p.ProviderType == (int)providerType)
                .OrderByDescending(p => p.RequestPickupDate)
                .ThenByDescending(p => p.ReadyByTime)
                .FirstOrDefault();
        }

        public int Store(ScheduledPickupDTO pickupResult,
            DateTime when,
            long? by)
        {
            var pickup = new ScheduledPickup()
            {
                ProviderType = pickupResult.ProviderType,
                RequestPickupDate = pickupResult.RequestPickupDate,
                RequestReadyByTime = pickupResult.RequestReadyByTime,
                RequestCloseTime = pickupResult.RequestCloseTime,
                ResultMessage = pickupResult.ResultMessage,
                SendRequestDate = pickupResult.SendRequestDate,

                ConfirmationNumber = pickupResult.ConfirmationNumber,
                PickupDate = pickupResult.PickupDate,
                ReadyByTime = pickupResult.ReadyByTime,
                PickupCharge = pickupResult.PickupCharge,
                CallInTime = pickupResult.CallInTime,

                CreateDate = when,
                CreatedBy = by
            };

            Add(pickup);
            unitOfWork.Commit();

            return pickup.Id;
        }
    }
}
