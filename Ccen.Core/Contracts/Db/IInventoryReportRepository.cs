using System;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Enums;
using Amazon.DTO;

namespace Amazon.Core.Contracts.Db
{
    public interface IInventoryReportRepository : IRepository<InventoryReport>
    {
        DateTime? GetLastProcessDate();
        DateTime? GetLastProcessDate(int type);
        InventoryReportDTO AddRequestedReport(string reportRequestId, int type, string marketplaceId);
        InventoryReportDTO AddSheduledReport(string reportId, int type, string marketplaceId, string reportName);
        InventoryReportDTO GetLastUnsaved(int type, string marketplaceId);
        string GetLastUnsavedId(int type, string marketplaceId);
        bool Update(string reportRequestId, string reportName, string reportId);
        //bool Update(InventoryReport report, string reportName);
        void MarkProcessedByRequestId(string reportRequestId, ReportProcessingResultType result);
        void MarkProcessedByReportId(string reportId, ReportProcessingResultType result);
    }
}
