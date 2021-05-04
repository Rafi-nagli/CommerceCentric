using Amazon.Core;
using Amazon.DAL;
using Ccen.Core.Contracts.Db.BulkEdit;
using Amazon.Core.Entities.BulkEdits;
using Ccen.DTO.BulkEdit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.DAL.Repositories.BulkEdits
{
    public class BulkEditHistoryRepository : Repository<BulkEditHistory>, IBulkEditHistoryRepository
    {
        public BulkEditHistoryRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IQueryable<BulkEditHistoryDTO> GetAllAsDto()
        {
            return GetAll().Select(x => new BulkEditHistoryDTO() 
            { 
                Id = x.Id,
                EntityId = x.EntityId,
                BulkEditOperationId = x.BulkEditOperationId,
                StyleId = x.StyleId,
                OperationDate = x.OperationDate,
                NewValue = x.NewValue,
                OldValue = x.OldValue
            });
        }
    }
}
