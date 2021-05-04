using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Models.Settings;
using Amazon.DTO.Users;

namespace Amazon.Model.Implementation
{
    public class CompanyService : ICompanyService
    {
        private IDbFactory _dbFactory;

        public CompanyService(IDbFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public EmailAccountDTO GetSmtpAccount(long companyId)
        {
            using (var db = _dbFactory.GetRDb())
            {
                return db.EmailAccounts.GetAllAsDto()
                    .FirstOrDefault(e => e.Type == (int) EmailAccountType.Smtp
                                && e.CompanyId == companyId);
            }
        }

        public EmailAccountDTO GetImapAccount(long companyId)
        {
            using (var db = _dbFactory.GetRDb())
            {
                return db.EmailAccounts.GetAllAsDto()
                    .FirstOrDefault(e => e.Type == (int)EmailAccountType.Imap
                    && e.CompanyId == companyId);
            }
        }

        public IList<ShipmentProviderDTO> GetShipmentProviders(long companyId)
        {
            using (var db = _dbFactory.GetRDb())
            {
                return db.ShipmentProviders.GetAllAsDto()
                    .Where(p => p.CompanyId == companyId).ToList();
            }
        }
    }
}
