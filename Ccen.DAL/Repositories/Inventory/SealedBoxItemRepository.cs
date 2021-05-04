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
    public class SealedBoxItemRepository : Repository<SealedBoxItem>, ISealedBoxItemRepository
    {
        public SealedBoxItemRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IList<SealedBoxItemDto> GetByBoxIdAsDto(long sealedBoxId)
        {
            var query = from b in unitOfWork.GetSet<SealedBoxItem>()
                join si in unitOfWork.GetSet<StyleItem>() on b.StyleItemId equals si.Id
                where b.BoxId == sealedBoxId
                select new SealedBoxItemDto()
                {
                    Id = b.Id,
                    BoxId = b.BoxId,
                    BreakDown = b.BreakDown,
                    StyleItemId = b.StyleItemId ?? 0,

                    Size = si.Size,
                    Color = si.Color,
                };

            return query.ToList();
        }

        private IEnumerable<SealedBoxItemDto> AsDto(IQueryable<SealedBoxItem> query)
        {
            return query.Select(b => new SealedBoxItemDto()
            {
                Id = b.Id,
                BoxId = b.BoxId,
                BreakDown = b.BreakDown,
                StyleItemId = b.StyleItemId ?? 0
            });
        }

        public IList<EntityUpdateStatus<long>> UpdateBoxItemsForBox(long boxId,
            IList<SealedBoxItemDto> boxItems,
            DateTime when,
            long? by)
        {
            var results = new List<EntityUpdateStatus<long>>();

            var items = boxItems;
            var dbExistItems = GetFiltered(b => b.BoxId == boxId).ToList();
            var newItems = boxItems.Where(b => b.Id == 0).ToList();

            foreach (var dbItem in dbExistItems)
            {
                var existItem = items.FirstOrDefault(b => b.Id == dbItem.Id);

                if (existItem != null)
                {
                    var hasChanges = dbItem.BreakDown != existItem.BreakDown;

                    if (hasChanges)
                    {
                        dbItem.StyleItemId = existItem.StyleItemId;
                        dbItem.BreakDown = existItem.BreakDown;
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
                var dbItem = new SealedBoxItem()
                {
                    BoxId = boxId,
                    StyleItemId = newItem.StyleItemId,
                    BreakDown = newItem.BreakDown,
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
