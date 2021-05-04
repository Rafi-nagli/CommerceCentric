using Amazon.Core;
using Amazon.Core.Contracts.Db.Cache;
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
    public class BulkEditOperationRepository : Repository<BulkEditOperation>, IBulkEditOperationRepository
    {
        public BulkEditOperationRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IQueryable<BulkEditOperationDTO> GetAllAsDto()
        {
            return GetAllAsDto(GetAll());
        }
        
        public long InsertOperation(BulkEditOperationDTO operation)
        {
            var o = new BulkEditOperation
            {
                CustomReportId = operation.CustomReportId,
                CustomReportPredefinedFieldId = operation.CustomReportPredefinedFieldId,
                ColumnName = operation.ColumnName,
                TableName = operation.TableName,
                OperatorId = (int)operation.Operator,
                ValueToSet = operation.ValueToSet,
                DateStart = DateTime.Now,
                CreatedBy = operation.CreatedBy,
                ExtraValue = operation.ExtraValue,
                DateLastUpdated = null,
                AllRecordsCnt = operation.AllRecordsCnt                
            };
            Add(o);
            unitOfWork.Commit();
            return o.Id;
        }

        public void UpdateOperation(long id, int successCnt, int warningCnt, int failedCnt)
        {
            var o = Get(id);
            o.DateEnd = DateTime.Now;
            o.SuccessCnt = successCnt;
            o.WarningCnt = warningCnt;
            o.FailedCnt = failedCnt;
            unitOfWork.Commit();
        }

        public IQueryable<BulkEditOperationDTO> GetAllAsDto(IQueryable<BulkEditOperation> query)
        {
            return query.Select(operation => new BulkEditOperationDTO()
            {
                Id = operation.Id,
                CustomReportId = operation.CustomReportId,
                CustomReportPredefinedFieldId = operation.CustomReportPredefinedFieldId,
                TableName = operation.TableName,
                ColumnName=operation.ColumnName,
                OperatorId = operation.OperatorId,
                ValueToSet = operation.ValueToSet,
                DateStart = operation.DateStart,
                CreatedBy = operation.CreatedBy,                
                DateEnd = operation.DateEnd,
                DateLastUpdated = operation.DateLastUpdated,
                SuccessCnt = operation.SuccessCnt,
                WarningCnt = operation.WarningCnt,
                FailedCnt = operation.FailedCnt,
                ExtraValue = operation.ExtraValue,
                AllRecordsCnt = operation.AllRecordsCnt,
                ProcessedRecordsCnt = operation.ProcessedRecordsCnt
            });
        }

        public BulkEditOperationDTO GetAsDTO(long id)
        {
            var operation = Get(id);
            return new BulkEditOperationDTO()
            {
                Id = operation.Id,
                CustomReportId = operation.CustomReportId,
                CustomReportPredefinedFieldId = operation.CustomReportPredefinedFieldId,
                TableName = operation.TableName,
                ColumnName = operation.ColumnName,
                OperatorId = operation.OperatorId,
                ValueToSet = operation.ValueToSet,
                DateStart = operation.DateStart,
                DateLastUpdated = operation.DateLastUpdated,
                CreatedBy = operation.CreatedBy,
                DateEnd = operation.DateEnd,
                SuccessCnt = operation.SuccessCnt,
                WarningCnt = operation.WarningCnt,
                FailedCnt = operation.FailedCnt,
                ExtraValue = operation.ExtraValue
            };
        }
    }
}
