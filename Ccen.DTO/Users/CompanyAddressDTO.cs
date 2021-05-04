using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Users
{
    public class CompanyAddressDTO
    {
        public long Id { get; set; }
        public long CompanyId { get; set; }

        public int Type { get; set; }

        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string Zip { get; set; }
        public string ZipAddon { get; set; }
        public string Phone { get; set; }

        public DateTime CreateDate { get; set; }

        public AddressDTO GetAddressDto()
        {
            return new AddressDTO()
            {
                Type = Type,

                Address1 = Address1,
                Address2 = Address2,
                City = City,
                State = State,
                Country = Country,
                Zip = Zip,
                ZipAddon = ZipAddon,
                Phone = Phone,
            };
        }
    }
}
