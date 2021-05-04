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
    public class OpenBoxItemRepository : Repository<OpenBoxItem>, IOpenBoxItemRepository
    {
        public OpenBoxItemRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IList<OpenBoxItemDto> GetByBoxIdAsDto(long openBoxId)
        {
            return AsDto(unitOfWork.GetSet<OpenBoxItem>().Where(b => b.BoxId == openBoxId)).ToList();
        }

        private IEnumerable<OpenBoxItemDto> AsDto(IQueryable<OpenBoxItem> query)
        {
            return query.Select(b => new OpenBoxItemDto()
            {
                Id = b.Id,
                BoxId = b.BoxId,
                Quantity = b.Quantity,
                StyleItemId = b.StyleItemId ?? 0
            });
        }

        public IList<EntityUpdateStatus<long>> UpdateBoxItemsForBox(long boxId,
            IList<OpenBoxItemDto> boxItems,
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
                    var hasChanges = dbItem.Quantity != existItem.Quantity;

                    if (hasChanges)
                    {
                        dbItem.StyleItemId = existItem.StyleItemId;
                        dbItem.Quantity = existItem.Quantity;
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
                var dbItem = new OpenBoxItem()
                {
                    BoxId = boxId,
                    StyleItemId = newItem.StyleItemId,
                    Quantity = newItem.Quantity,
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
