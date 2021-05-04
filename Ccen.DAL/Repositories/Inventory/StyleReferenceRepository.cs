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
    public class StyleReferenceRepository : Repository<StyleReference>, IStyleReferenceRepository
    {
        public StyleReferenceRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IList<StyleReferenceDTO> GetByStyleId(long styleId)
        {
            return GetAllAsDto().Where(s => s.StyleId == styleId).ToList();
        }

        public IQueryable<StyleReferenceDTO> GetAllAsDto()
        {
            var query = from sRef in unitOfWork.GetSet<StyleReference>()
                        join s in unitOfWork.GetSet<Style>() on sRef.LinkedStyleId equals s.Id
                        select new StyleReferenceDTO()
                        {
                            Id = sRef.Id,
                            StyleId = sRef.StyleId,
                            LinkedStyleId = s.Id,
                            LinkedStyleString = s.StyleID,
                            Price = sRef.Price,
                        };

            return query;
        }

        public void UpdateStyleReferencesForStyle(long styleId,
            IList<StyleReferenceDTO> linkedStyles,
            DateTime when,
            long? by)
        {
            var items = linkedStyles;
            var dbExistItems = GetFiltered(l => l.StyleId == styleId).ToList();
            var newItems = linkedStyles.Where(l => l.Id == 0).ToList();

            foreach (var dbItem in dbExistItems)
            {
                var existItem = items.FirstOrDefault(l => l.Id == dbItem.Id);
                if (existItem != null)
                {
                    dbItem.LinkedStyleId = existItem.LinkedStyleId;
                    dbItem.StyleId = styleId;
                    dbItem.Price = existItem.Price;

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
                var dbItem = new StyleReference()
                {
                    StyleId = styleId,
                    LinkedStyleId = newItem.LinkedStyleId,
                    Price = newItem.Price,

                    CreateDate = when,
                    CreatedBy = by
                };
                Add(dbItem);

                newItem.Id = dbItem.Id;
            }

            unitOfWork.Commit();
        }
    }
}
