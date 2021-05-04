using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.DAL;
using Amazon.DTO.Users;
using Amazon.Web.ViewModels.Status;

namespace Amazon.Web.ViewModels
{
    public class AccountStatusViewModel
    {
        public IList<ShipmentProviderViewModel> ShipmentProviderList { get; set; }


        public AccountStatusViewModel(CompanyDTO company)
        {
            if (company == null)
            {
                ShipmentProviderList = new List<ShipmentProviderViewModel>();
            }
            else
            {
                ShipmentProviderList = company.ShipmentProviderInfoList.Select(p => new ShipmentProviderViewModel()
                {
                    Balance = p.Balance,
                    Type = p.Type,
                    ShortName = p.ShortName ?? p.Name
                }).ToList();
            }
        }
    }
}