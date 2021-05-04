using System.Linq;
using Amazon.Core.Contracts.Db;

using Amazon.Core.Entities.Users;

using Amazon.Core;
using Amazon.DTO.Users;

namespace Amazon.DAL.Repositories
{
    public class CompanyRepository : Repository<Company>, ICompanyRepository
    {
        public CompanyRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IQueryable<CompanyDTO> GetAllAsDto()
        {
            return AsDto(unitOfWork.GetSet<Company>());
        }

        public CompanyDTO GetByNameAsDto(string name)
        {
            return GetAllAsDto().FirstOrDefault(c => c.CompanyName == name);
        }

        public CompanyDTO GetByNameWithSettingsAsDto(string name)
        {
            var company = GetByNameAsDto(name);
            LoadSettings(company);

            return company;
        }

        public CompanyDTO GetFirstWithSettingsAsDto()
        {
            var company = GetAllAsDto().FirstOrDefault();
            LoadSettings(company);

            return company;
        }

        public CompanyDTO GetByIdWithSettingsAsDto(long id)
        {
            var company = GetAllAsDto().FirstOrDefault(c => c.Id == id);
            LoadSettings(company);

            return company;
        }

        private void LoadSettings(CompanyDTO company)
        {
            company.ShipmentProviderInfoList = unitOfWork.ShipmentProviders.GetByCompanyId(company.Id);
            company.AddressProviderInfoList = unitOfWork.AddressProviders.GetByCompanyId(company.Id);
            company.EmailAccounts = unitOfWork.EmailAccounts.GetByCompanyId(company.Id);
            company.SQSAccounts = unitOfWork.SQSAccounts.GetByCompanyId(company.Id);
            company.AddressList = unitOfWork.CompanyAddresses.GetByCompanyId(company.Id);
        }
        
        private IQueryable<CompanyDTO> AsDto(IQueryable<Company> query)
        {
            return query.Select(i => new CompanyDTO()
            {
                Id = i.Id,
                CompanyName = i.CompanyName,
                ShortName = i.ShortName,

                FullName = i.FullName,
                Address1 = i.Address1,
                Address2 = i.Address2,
                City = i.City,
                State = i.State,
                Country = i.Country,
                Zip = i.Zip,
                ZipAddon = i.ZipAddon,
                Phone = i.Phone,

                AmazonFeedMerchantIdentifier = i.AmazonFeedMerchantIdentifier,
                MelissaCustomerId = i.MelissaCustomerId,
                USPSUserId = i.USPSUserId,
                CanadaPostKeys = i.CanadaPostKeys,

                SellerName = i.SellerName,
                SellerEmail = i.SellerEmail,

                SellerAlertName = i.SellerAlertName,
                SellerAlertEmail = i.SellerAlertEmail,

                SellerWarehouseEmailName = i.SellerWarehouseEmailName,
                SellerWarehouseEmailAddress = i.SellerWarehouseEmailAddress,

                IsActive = i.IsActive,
                IsDeleted = i.IsDeleted
            });
        }
    }
}
