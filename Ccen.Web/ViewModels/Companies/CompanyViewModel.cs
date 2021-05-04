using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.DTO.Users;

namespace Amazon.Web.ViewModels.Companies
{
    public class CompanyViewModel
    {
        public string CompanyName { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string ZipAddon { get; set; }
        public string Country { get; set; }

        public string Phone { get; set; }

        public CompanyViewModel()
        {
            
        }

        public CompanyViewModel(CompanyDTO company)
        {
            
        }
    }
}