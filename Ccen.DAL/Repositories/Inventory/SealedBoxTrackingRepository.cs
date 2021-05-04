using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Contracts.Db.Inventory;
using Amazon.Core.Entities.Inventory;
using Amazon.Core;
using Amazon.Core.Models;
using Amazon.DTO.Inventory;

namespace Amazon.DAL.Repositories.Inventory
{
    public class SealedBoxTrackingRepository : Repository<SealedBoxTracking>, ISealedBoxTrackingRepository
    {
        public SealedBoxTrackingRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IList<SealedBoxTrackingDTO> GetByBoxIdAsDto(long openBoxId)
        {
            return AsDto(unitOfWork.GetSet<SealedBoxTracking>().Where(b => b.BoxId == openBoxId)).ToList();
        }

        private IEnumerable<SealedBoxTrackingDTO> AsDto(IQueryable<SealedBoxTracking> query)
        {
            return query.Select(b => new SealedBoxTrackingDTO()
            {
                Id = b.Id,
                BoxId = b.BoxId,
                TrackingNumber = b.TrackingNumber,
                Carrier = b.Carrier,
                EstDeliveryDate = b.EstDeliveryDate,
            });
        }

        public IList<EntityUpdateStatus<long>> UpdateBoxTrackingsForBox(long boxId,
            IList<SealedBoxTrackingDTO> boxTrackings,
            DateTime when,
            long? by)
        {
            var results = new List<EntityUpdateStatus<long>>();

            var items = boxTrackings;
            var dbExistItems = GetFiltered(b => b.BoxId == boxId).ToList();
            var newItems = boxTrackings.Where(b => b.Id == 0).ToList();

            foreach (var dbItem in dbExistItems)
            {
                var existItem = items.FirstOrDefault(b => b.Id == dbItem.Id);

                if (existItem != null)
                {
                    var hasChanges = dbItem.TrackingNumber != existItem.TrackingNumber
                            || dbItem.Carrier != existItem.Carrier
                            || dbItem.EstDeliveryDate != existItem.EstDeliveryDate;

                    if (hasChanges)
                    {
                        dbItem.TrackingNumber = existItem.TrackingNumber;
                        dbItem.Carrier = existItem.Carrier;
                        dbItem.EstDeliveryDate = existItem.EstDeliveryDate;
                        dbItem.UpdateDate = when;
                        dbItem.UpdatedBy = by;

                        results.Add(new EntityUpdateStatus<long>(dbItem.Id, UpdateType.Update));
                    }
                }
                else
                {
                    Remove(dbItem);
                    results.Add(new EntityUpdateStatus<long>(dbItem.Id, UpdateType.Removed));
                }
            }

            unitOfWork.Commit();

            foreach (var newItem in newItems)
            {
                var dbItem = new SealedBoxTracking()
                {
                    BoxId = boxId,
                    TrackingNumber = newItem.TrackingNumber,
                    Carrier = newItem.Carrier,
                    EstDeliveryDate = newItem.EstDeliveryDate,
                    CreateDate = when,
                    CreatedBy = by,
                };
                Add(dbItem);
                unitOfWork.Commit();

                newItem.Id = dbItem.Id;

                results.Add(new EntityUpdateStatus<long>(dbItem.Id, UpdateType.Insert));
            }

            return results;
        }
    }
}
