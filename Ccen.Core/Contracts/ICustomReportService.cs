using Amazon.Core.Entities.CustomReports;
using Amazon.Core.Entities.DropShippers;
using Amazon.Core.Models.DropShippers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Amazon.Core.Contracts
{
    public interface ICustomReportService
    {
        IList<CustomReportPredefinedField> GetPredefinedFieldsListForEntity(string entityName);
        IList<CustomReportPredefinedField> GetPredefinedFieldsListAll();        
    }
}
