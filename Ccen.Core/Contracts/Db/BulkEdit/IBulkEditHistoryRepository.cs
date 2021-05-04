using Amazon.Core;
using Amazon.Core.Entities.BulkEdits;
using Ccen.DTO.BulkEdit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccen.Core.Contracts.Db.BulkEdit
{
    public interface IBulkEditHistoryRepository : IRepository<BulkEditHistory>
    {
        IQueryable<BulkEditHistoryDTO> GetAllAsDto();        
    }
}
