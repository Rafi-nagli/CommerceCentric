using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Core.Entities.Enums;
using Amazon.Core;
using Amazon.DTO;

namespace Amazon.DAL.Repositories
{
    public class InventoryReportRepository : Repository<InventoryReport>, IInventoryReportRepository
    {
        public InventoryReportRepository(IQueryableUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public DateTime? GetLastProcessDate()
        {
            var lastReport = GetAll().OrderByDescending(r => r.ProcessDate).FirstOrDefault();
            return lastReport != null
                ? lastReport.ProcessDate
                : null;
        }

        public DateTime? GetLastProcessDate(int type)
        {
            var lastReport = GetAll().Where(r => r.TypeId == type && r.ProcessDate.HasValue).OrderByDescending(r => r.ProcessDate).FirstOrDefault();
            return lastReport != null
                ? lastReport.ProcessDate
                : null;
        }

        public InventoryReportDTO AddRequestedReport(string reportRequestId, int type, string marketplaceId)
        {
            var report = new InventoryReport
            {
                RequestIdentifier = reportRequestId,
                RequestDate = DateTime.UtcNow,
                TypeId = type,
                MarketplaceId = marketplaceId,
            };
            Add(report);
            unitOfWork.Commit();
            return AsDto((new[] { report }).AsQueryable()).FirstOrDefault();
        }

        public InventoryReportDTO AddSheduledReport(string reportId, int type, string marketplaceId, string reportName)
        {
            var report = new InventoryReport
            {
                ReportIdentifier = reportId,
                RequestDate = DateTime.UtcNow,
                TypeId = type,
                MarketplaceId = marketplaceId,
            };
            Add(report);
            unitOfWork.Commit();
            return AsDto((new[] { report }).AsQueryable()).FirstOrDefault();
        }

        public InventoryReportDTO GetLastUnsaved(int type, string marketplaceId)
        {
            return AsDto(GetFiltered(r => r.Path == null 
                    && r.TypeId == type 
                    && r.MarketplaceId == marketplaceId
                    && !r.ProcessDate.HasValue))
                .OrderByDescending(r => r.RequestDate)
                .FirstOrDefault();
        }

        public string GetLastUnsavedId(int type, string marketplaceId)
        {
            var unprocessed = GetLastUnsaved(type, marketplaceId);
            return unprocessed != null
                ? unprocessed.RequestIdentifier
                : string.Empty;
        }

        public bool Update(string reportRequestId, 
            string reportName,
            string reportId)
        {
            var report = GetByRequestId(reportRequestId);
            if (report == null)
            {
                return false;
            }
            report.Path = reportName;
            report.ReportIdentifier = reportId;
            unitOfWork.Commit();
            return true;
        }

        private InventoryReport GetByRequestId(string requestId)
        {
            return GetFiltered(r => r.RequestIdentifier == requestId).FirstOrDefault();
        }

        private InventoryReport GetByReportId(string reportId)
        {
            return GetFiltered(r => r.ReportIdentifier == reportId).FirstOrDefault();
        }

        public void MarkProcessedByRequestId(string reportRequestId, ReportProcessingResultType result)
        {
            var report = GetByRequestId(reportRequestId);
            if (report != null)
            {
                report.ProcessDate = DateTime.UtcNow;
                report.ProcessResult = (int)result;
                unitOfWork.Commit();
            }
        }

        public void MarkProcessedByReportId(string reportId, ReportProcessingResultType result)
        {
            var report = GetByReportId(reportId);
            if (report != null)
            {
                report.ProcessDate = DateTime.UtcNow;
                report.ProcessResult = (int)result;
                unitOfWork.Commit();
            }
        }

        private IQueryable<InventoryReportDTO> AsDto(IQueryable<InventoryReport> query)
        {
            return query.Select(r => new InventoryReportDTO()
            {
                Id = r.Id,
                TypeId = r.TypeId,
                MarketplaceId = r.MarketplaceId,
                Path = r.Path,
                ProcessDate = r.ProcessDate,
                ProcessResult = r.ProcessResult,
                RequestDate = r.RequestDate,
                RequestIdentifier = r.RequestIdentifier
            });
        }
    }
}
