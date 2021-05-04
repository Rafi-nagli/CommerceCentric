using System;
using System.Collections.Generic;
using Amazon.Core.Functions;
using Amazon.Core.Views;
using Amazon.DTO.Reports;

namespace Amazon.Core.Contracts.Db
{
    public interface IReportRepository
    {
        IList<SalesReportDTO> GetSalesByDateReport(DateTime fromDate);
        IList<SalesReportDTO> GetSalesByFeatureReport(DateTime fromDate, int featureId);
    }
}
