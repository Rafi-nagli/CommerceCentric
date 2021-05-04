
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Enums;
using Amazon.DTO;

namespace Amazon.Core.Contracts
{
    public interface IAmazonReportService
    {
        AmazonReportType ReportType { get; }
        string ReportFileName { get; }
        string ReportRequestId { get; }
        string ReportId { get; }

        int SaveReportAttempts { get; }
        

        IList<ReportDTO> GetReportList();
        bool RequestReport();
        ReportRequestDTO GetRequestStatus();

        void UseReportRequestId(string reportRequestId);
        bool SaveRequestedReport();
        bool SaveScheduledReport(string reportId);
        bool ProcessReport();
        bool ProcessReport(IList<string> asinList);
    }
}
