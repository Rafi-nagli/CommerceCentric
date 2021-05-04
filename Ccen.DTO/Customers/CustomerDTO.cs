using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Customers
{
    public class CustomerDTO
    {
        public long? Id { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string Zip { get; set; }
        public string ZipAddon { get; set; }
        public string State { get; set; }
        public string Country { get; set; }

        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }

        public DateTime? CreateDate { get; set; }

        //Additionals
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string SourceMarketId { get; set; }
    }
}
