using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.Users
{
    public class Company : BaseDateAndByEntity
    {
        [Key]
        public long Id { get; set; }
        public string CompanyName { get; set; }
        public string ShortName { get; set; }

        public string FullName { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string Zip { get; set; }
        public string ZipAddon { get; set; }
        public string Phone { get; set; }
        

        public string AmazonFeedMerchantIdentifier { get; set; }
        public string MelissaCustomerId { get; set; }

        public string USPSUserId { get; set; }
        public string CanadaPostKeys { get; set; }

        public string SellerName { get; set; }
        public string SellerEmail { get; set; }
        
        public string SellerAlertName { get; set; }
        public string SellerAlertEmail { get; set; }

        public string SellerWarehouseEmailName { get; set; }
        public string SellerWarehouseEmailAddress { get; set; }

        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
    }
}
