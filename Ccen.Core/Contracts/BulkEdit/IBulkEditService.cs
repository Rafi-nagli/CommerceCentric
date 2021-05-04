using Amazon.DTO.CustomReports;
using Amazon.Core.Entities.BulkEdits;
using Ccen.DTO.BulkEdit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccen.Core.Contracts.BulkEdit
{
    public interface IBulkEditService
    {
        BulkEditResult AddOperation(List<long> styleIds, BulkEditOperationDTO operation);
        void UpdateOperation(long operation, int succesCnt, int warningCnt, int updateCnt);
        BulkEditResult CancelOperation(long operationId);
        long AddOperationHistory(BulkEditHistoryDTO history);
        IQueryable<BulkEditOperationDTO> GetAllOperations();
        IList<BulkEditHistoryDTO> GetHistoriesByOperationId(long operationId);
        /*BulkEditResult EditEntityByStyleId(long styleId, CustomReportPredefinedFieldDTO entityData);
        IList<int> GetEntityIds(long styleId, CustomReportPredefinedFieldDTO entityData);*/
    }
}
