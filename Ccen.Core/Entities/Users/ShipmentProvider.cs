using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.Users
{
    public class ShipmentProvider
    {
        [Key]
        public long Id { get; set; }

        public long CompanyId { get; set; }

        public int Type { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }

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
    }
}
