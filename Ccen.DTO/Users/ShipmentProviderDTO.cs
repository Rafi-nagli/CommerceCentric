using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.DTO.Contracts;

namespace Amazon.DTO.Users
{
    public class ShipmentProviderDTO : IStampsAuthInfo
    {
        public long Id { get; set; }

        public long CompanyId { get; set; }

        public string Name { get; set; }
        public string ShortName { get; set; }
        public int Type { get; set; }

        public string Key1 { get; set; }
        public string Key2 { get; set; }
        public string Key3 { get; set; }
        public string Key4 { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public string EndPointUrl { get; set; }

        public decimal Balance { get; set; }
        public DateTime? BalanceUpdateDate { get; set; }

        public bool IsActive { get; set; }
        public DateTime CreateDate { get; set; }


        #region IStampsAuthInfo
        public int AccountType
        {
            get { return Type; }
        }

        public string StampIntegration
        {
            get { return Key1; }
        }

        public string StampUsername
        {
            get { return UserName; }
        }

        public string StampPassword
        {
            get { return Password; }
        }
        #endregion
    }
}
