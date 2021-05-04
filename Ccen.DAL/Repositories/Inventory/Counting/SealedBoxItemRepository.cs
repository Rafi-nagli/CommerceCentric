using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Contracts.Db.Inventory;
using Amazon.Core.Entities.Inventory;
using Amazon.Core;
using Amazon.DTO.Inventory;

namespace Amazon.DAL.Repositories.Inventory
{
    public class SealedBoxCountingItemRepository : Repository<SealedBoxCountingItem>, ISealedBoxCountingItemRepository
    {
        public SealedBoxCountingItemRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IList<SealedBoxCountingItemDto> GetByBoxIdAsDto(long sealedBoxId)
        {
            var query = from b in unitOfWork.GetSet<SealedBoxCountingItem>()
                join si in unitOfWork.GetSet<StyleItem>() on b.StyleItemId equals si.Id
                where b.BoxId == sealedBoxId
                        select new SealedBoxCountingItemDto()
                {
                    Id = b.Id,
                    BoxId = b.BoxId,
                    BreakDown = b.BreakDown,
                    StyleItemId = b.StyleItemId,

                    Size = si.Size,
                    Color = si.Color,
                };

            return query.ToList();
        }

        private IEnumerable<SealedBoxCountingItemDto> AsDto(IQueryable<SealedBoxCountingItem> query)
        {
            return query.Select(b => new SealedBoxCountingItemDto()
            {
                Id = b.Id,
                BoxId = b.BoxId,
                BreakDown = b.BreakDown,
                StyleItemId = b.StyleItemId
            });
        }

        public void UpdateBoxItemsForBox(long boxId,
            IList<SealedBoxCountingItemDto> boxItems,
            DateTime when,
            long? by)
        {
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
                    }
                }
                else
                {
                    Remove(dbItem);
                }
            }

            unitOfWork.Commit();

            foreach (var newItem in newItems)
            {
                var dbItem = new SealedBoxCountingItem()
                {
                    BoxId = boxId,
                    StyleItemId = newItem.StyleItemId,
                    BreakDown = newItem.BreakDown,
                    CreateDate = when,
                    CreatedBy = by,
                };
                Add(dbItem);
            }

            unitOfWork.Commit();
        }
    }
}
