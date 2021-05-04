using System.Linq;
using Amazon.Core.Entities.Users;
using Amazon.DTO.Users;

namespace Amazon.Core.Contracts.Db
{
    public interface ICompanyRepository : IRepository<Company>
    {
        IQueryable<CompanyDTO> GetAllAsDto();
        CompanyDTO GetByNameAsDto(string name);
        CompanyDTO GetByNameWithSettingsAsDto(string name);
        CompanyDTO GetByIdWithSettingsAsDto(long id);
        CompanyDTO GetFirstWithSettingsAsDto();
    }
}
