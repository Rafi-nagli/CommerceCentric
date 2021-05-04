using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Contracts.Db.Inventory;
using Amazon.Core.Entities.Inventory;
using Amazon.Core;
using Amazon.DTO.Inventory;

namespace Amazon.DAL.Repositories.Inventory
{
    public class OpenBoxCountingItemRepository : Repository<OpenBoxCountingItem>, IOpenBoxCountingItemRepository
    {
        public OpenBoxCountingItemRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IList<OpenBoxCountingItemDto> GetByBoxIdAsDto(long openBoxId)
        {
            return AsDto(unitOfWork.GetSet<OpenBoxCountingItem>().Where(b => b.BoxId == openBoxId)).ToList();
        }

        private IEnumerable<OpenBoxCountingItemDto> AsDto(IQueryable<OpenBoxCountingItem> query)
        {
            return query.Select(b => new OpenBoxCountingItemDto()
            {
                Id = b.Id,
                BoxId = b.BoxId,
                Quantity = b.Quantity,
                StyleItemId = b.StyleItemId,

                Size = b.Size,
                Color = b.Color,
            });
        }

        public void UpdateBoxItemsForBox(long boxId,
            IList<OpenBoxCountingItemDto> boxItems,
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
                    var hasChanges = dbItem.Quantity != existItem.Quantity;

                    if (hasChanges)
                    {
                        dbItem.StyleItemId = existItem.StyleItemId;
                        dbItem.Quantity = existItem.Quantity;
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
                var dbItem = new OpenBoxCountingItem()
                {
                    BoxId = boxId,
                    StyleItemId = newItem.StyleItemId,
                    Quantity = newItem.Quantity,

                    Size = newItem.Size,
                    Color = newItem.Color,

                    CreateDate = when,
                    CreatedBy = by,
                };
                Add(dbItem);
            }

            unitOfWork.Commit();
        }
    }
}
