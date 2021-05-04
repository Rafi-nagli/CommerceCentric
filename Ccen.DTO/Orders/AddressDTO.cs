using System;

namespace Amazon.DTO
{
    public class AddressDTO
    {
        public int? Type { get; set; }

        public string FullName { get; set; }

        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string Zip { get; set; }
        public string ZipAddon { get; set; }

        public bool IsManuallyUpdated { get; set; }
        public bool? IsResidential { get; set; }

        public string ManuallyFullName { get; set; }
        public string ManuallyAddress1 { get; set; }
        public string ManuallyAddress2 { get; set; }
        public string ManuallyCity { get; set; }
        public string ManuallyState { get; set; }
        public string ManuallyCountry { get; set; }
        public string ManuallyZip { get; set; }
        public string ManuallyZipAddon { get; set; }
        public string ManuallyPhone { get; set; }

        public string Phone { get; set; }
        public DateTime? ShipDate { get; set; }

        public int ValidationStatus { get; set; }
        public bool IsVerified { get; set; }
        public string AddressKey { get; set; }

        public string CompanyName { get; set; }
        public string ContactName { get; set; }

        public string BuyerEmail { get; set; }

        public string FinalFullName
        {
            get
            {
                if (!String.IsNullOrEmpty(ManuallyFullName)
                    && IsManuallyUpdated)
                    return ManuallyFullName;
                return FullName;
            }
        }

        public string FinalAddress1
        {
            get
            {
                return IsManuallyUpdated ? ManuallyAddress1 : Address1;
            }
        }

        public string FinalAddress2
        {
            get
            {
                return IsManuallyUpdated ? ManuallyAddress2 : Address2;
            }
        }

        public string FinalCity
        {
            get
            {
                return IsManuallyUpdated ? ManuallyCity : City;
            }
        }

        public string FinalState
        {
            get
            {
                return IsManuallyUpdated ? ManuallyState : State;
            }
        }

        public string FinalCountry
        {
            get
            {
                return IsManuallyUpdated ? ManuallyCountry : Country;
            }
        }

        public string FinalZip
        {
            get
            {
                return IsManuallyUpdated ? ManuallyZip : Zip;
            }
        }

        public string FinalZipAddon
        {
            get
            {
                return IsManuallyUpdated ? ManuallyZipAddon : ZipAddon;
            }
        }

        public string FinalPhone
        {
            get
            {
                return IsManuallyUpdated ? ManuallyPhone : Phone;
            }
        }

        public override string ToString()
        {
            return "FullName=" + FullName + 
                   ", Address1=" + Address1 +
                   ", Address2=" + Address2 +
                   ", City=" + City +
                   ", State=" + State +
                   ", Country=" + Country +
                   ", Zip=" + Zip +
                   ", ZipAddon=" + ZipAddon +
                   ", Phone=" + Phone +

                   ", AddressKey=" + AddressKey +
                   ", ManuallyAddress1=" + ManuallyAddress1 +
                   ", ManuallyAddress2=" + ManuallyAddress2 +
                   ", ManuallyCity=" + ManuallyCity +
                   ", ManuallyState=" + ManuallyState +
                   ", ManuallyCountry=" + ManuallyCountry +
                   ", ManuallyZip=" + ManuallyZip +
                   ", ManuallyZipAddon=" + ManuallyZipAddon +
                   ", ManuallyPhone=" + ManuallyPhone +
                   
                   ", ShipDate=" + (ShipDate.HasValue ? ShipDate.Value.ToString() : "[null]");
        }
    }
}
