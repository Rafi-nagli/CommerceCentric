using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Models.Settings;
using Amazon.DTO.Users;

namespace Amazon.Common.Helpers
{
    public static class CompanyHelper
    {

        public static EmailAccountDTO GetSmtpSettings(CompanyDTO company)
        {
            if (company == null)
                return null;
            if (company.EmailAccounts == null)
                return null;

            return company.EmailAccounts.FirstOrDefault(a => a.Type == (int) EmailAccountType.Smtp);
        }

        public static void UpdateBalance(IUnitOfWork db,
            CompanyDTO company, 
            IList<IShipmentApi> labelProviders,
            bool withRequestUpdates,
            DateTime? when)
        {
            if (labelProviders == null)
                return;

            if (company.ShipmentProviderInfoList == null)
                return;

            var providerList = company.ShipmentProviderInfoList;

            foreach (var labelProvider in labelProviders)
            {
                decimal balance = labelProvider.Balance;
                if (withRequestUpdates)
                {
                    try
                    {
                        var balanceResult = labelProvider.GetBalance();
                        if (balanceResult != null)
                        {
                            balance = balanceResult.Balance;
                            db.ShipmentProviders.UpdateBalance(labelProvider.ProviderId, balance, when);
                        }
                    }
                    catch (Exception ex)
                    {
                        
                    }
                }
                else
                {
                    balance = labelProvider.Balance;
                }

                var companyProvider = providerList.FirstOrDefault(p => p.Id == labelProvider.ProviderId);
                if (companyProvider != null)
                {
                    companyProvider.Balance = balance;
                    companyProvider.BalanceUpdateDate = when;
                }
            }
        }
    }
}
