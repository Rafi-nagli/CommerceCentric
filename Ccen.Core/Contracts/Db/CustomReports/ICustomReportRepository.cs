using System.Linq;
using Amazon.Core.Entities;
using Amazon.Core.Entities.CustomReports;
using Amazon.Core.Entities.DropShippers;
using Amazon.Core.Entities.Users;
using Amazon.Core.Models;
using Amazon.DTO.CustomReports;


namespace Amazon.Core.Contracts.Db
{
    public interface ICustomReportRepository : IRepository<CustomReport>
    {
        IQueryable<CustomReportDTO> GetAllAsDto();
    }
}
