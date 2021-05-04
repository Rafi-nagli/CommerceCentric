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
    public interface IBulkEditOperationRepository : IRepository<BulkEditOperation>
    {
        IQueryable<BulkEditOperationDTO> GetAllAsDto();
        BulkEditOperationDTO GetAsDTO(long id);
        long InsertOperation(BulkEditOperationDTO operation);
        void UpdateOperation(long id, int successCnt, int warningCnt, int failedCnt);
        IQueryable<BulkEditOperationDTO> GetAllAsDto(IQueryable<BulkEditOperation> query);        
    }
}
