using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.DropShippers
{
    public class DropShipperDTO
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string IncomeFTPFolder { get; set; }
        public string IncomeArchiveFolder { get; set; }

        public string OutputFTPFolder { get; set; }

        public string OverrideEmail { get; set; }

        public string ItemFeedNotifyEmails { get; set; }
        public string InventoryFeedNotifyEmails { get; set; }
        public string OrderFeedNotifyEmails { get; set; }
        public string NotifiesBccEmails { get; set; }

        public string AmazonSFPProfile { get; set; }

        public int ItemIdMode { get; set; }
        public int CostMode { get; set; }
        public decimal? CostMultiplier { get; set; }
        public int PriceMode { get; set; }

        public int? QuantityThreshold { get; set; }
        public int? ProcessingThresholdTime { get; set; }

        public string AddressName { get; set; }

        public string Address1 { get; set; }
        public string Address2 { get; set; }

        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string Zip { get; set; }
        public string ZipAddon { get; set; }

        public string Phone { get; set; }
        public string Email { get; set; }

        public AddressDTO GetAddressDto()
        {
            return new AddressDTO()
            {
                FullName = AddressName,
                Address1 = Address1,
                Address2 = Address2,
                City = City,
                State = State,
                Country = Country,
                Zip = Zip,
                ZipAddon = ZipAddon,
                Phone = Phone,
                BuyerEmail = Email,
            };
        }


        public int PrePriority { get; set; }
        public int PostPriority { get; set; }

        public int SortOrder { get; set; }

        public bool EnableListingFeedProcessing { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
