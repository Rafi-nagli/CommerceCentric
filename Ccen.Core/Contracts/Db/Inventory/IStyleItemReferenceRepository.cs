using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core.Entities.Inventory;
using Amazon.DTO.Inventory;

namespace Amazon.Core.Contracts.Db.Inventory
{
    public interface IStyleItemReferenceRepository : IRepository<StyleItemReference>
    {
        IList<StyleItemReferenceDTO> GetByStyleId(long styleId);
        IQueryable<StyleItemReferenceDTO> GetAllAsDto();

        void UpdateStyleItemReferencesForStyle(long styleId,
            IList<StyleItemReferenceDTO> linkedStyles,
            DateTime when,
            long? by);
    }
}
