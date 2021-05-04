using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.DTO;

namespace Amazon.Core.Models
{
    public class TrackingRecord
    {
        public TrackingStatusSources Source { get; set; }
        public DateTime? Date { get; set; }
        public int Type { get; set; }

        public string Country { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string Zip { get; set; }

        public string Message { get; set; }

        public AddressDTO AsAddressDto()
        {
            return new AddressDTO()
            {
                Country = Country,
                State = State,
                City = City,
                Zip = Zip
            };
        }
    }
}
