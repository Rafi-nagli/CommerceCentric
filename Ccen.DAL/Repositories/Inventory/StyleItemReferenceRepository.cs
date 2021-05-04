using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core;
using Amazon.Core.Contracts.Db.Inventory;
using Amazon.Core.Entities.Inventory;
using Amazon.DTO.Inventory;

namespace Amazon.DAL.Repositories.Inventory
{
    public class StyleItemReferenceRepository : Repository<StyleItemReference>, IStyleItemReferenceRepository
    {
        public StyleItemReferenceRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IList<StyleItemReferenceDTO> GetByStyleId(long styleId)
        {
            return GetAllAsDto().Where(s => s.StyleId == styleId).ToList();
        }

        public IQueryable<StyleItemReferenceDTO> GetAllAsDto()
        {
            var query = from siRef in unitOfWork.GetSet<StyleItemReference>()
                        select new StyleItemReferenceDTO()
                        {
                            Id = siRef.Id,
                            StyleId = siRef.StyleId,

                            StyleItemId = siRef.StyleItemId,
                            LinkedStyleItemId = siRef.LinkedStyleItemId,
                        };

            return query;
        }

        public void UpdateStyleItemReferencesForStyle(long styleId,
            IList<StyleItemReferenceDTO> linkedStyleItems,
            DateTime when,
            long? by)
        {
            var items = linkedStyleItems;
            var dbExistItems = GetFiltered(l => l.StyleId == styleId).ToList();
            var newItems = linkedStyleItems.Where(l => l.Id == 0).ToList();

            foreach (var dbItem in dbExistItems)
            {
                var existItem = items.FirstOrDefault(l => l.Id == dbItem.Id);
                if (existItem != null)
                {
                    dbItem.StyleId = styleId;
                    dbItem.StyleItemId = existItem.StyleItemId;
                    dbItem.LinkedStyleItemId = existItem.LinkedStyleItemId;
                    
                    dbItem.UpdateDate = when;
                    dbItem.UpdatedBy = by;
                }
                else
                {
                    Remove(dbItem);
                }
            }

            foreach (var newItem in newItems)
            {
                var dbItem = new StyleItemReference()
                {
                    StyleId = styleId,
                    StyleItemId = newItem.StyleItemId,
                    LinkedStyleItemId = newItem.LinkedStyleItemId,

                    CreateDate = when,
                    CreatedBy = by
                };
                Add(dbItem);
            }

            unitOfWork.Commit();
        }
    }
}
