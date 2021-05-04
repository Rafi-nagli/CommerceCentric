using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core.Entities.Inventory;
using Amazon.DTO.Inventory;

namespace Amazon.Core.Contracts.Db.Inventory
{
    public interface IStyleReferenceRepository : IRepository<StyleReference>
    {
        IList<StyleReferenceDTO> GetByStyleId(long styleId);
        IQueryable<StyleReferenceDTO> GetAllAsDto();

        void UpdateStyleReferencesForStyle(long styleId,
            IList<StyleReferenceDTO> linkedStyles,
            DateTime when,
            long? by);
    }
}
