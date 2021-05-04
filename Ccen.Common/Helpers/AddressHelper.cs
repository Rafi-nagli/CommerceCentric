using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Amazon.Core.Models;
using Amazon.Core.Models.Settings;
using Amazon.DTO;
using Amazon.DTO.Users;
using Amazon.Utils;

namespace Amazon.Common.Helpers
{
    public class AddressHelper
    {
        public static bool IsAPO(string country, 
            string state,
            string city)
        {
            if (state == "AA"
                || state == "AE"
                || state == "AP"
                || city == "APO"
                || city == "FPO"
                || city == "DPO")
                return true;
            return false;
        }

        public static bool IsUSIsland(string country, string state)
        {
            //from Guam, Virgin Islands, Marshal Islands etc for Postal service
            if (country == "AS"
                || country == "DC"
                || country == "FM"
                || country == "GU"
                || country == "MH"
                || country == "MP"
                || country == "PW"
                || country == "PR"
                || country == "VI"

                || state == "AS"
                //|| state == "DC"
                || state == "FM"
                || state == "GU"
                || state == "MH"
                || state == "MP"
                || state == "PW"
                || state == "PR"
                || state == "VI"
                )
                return true;
            return false;
        }

        public static bool IsPOBox(string address1, string address2)
        {
            var line1 = address1 ?? "";
            var line2 = address2 ?? "";

            if (line1.Contains("Reference")
                || line2.Contains("Reference"))
                return false;

            var poRegex = new Regex(@"(P|p)[.\\/\s]*(O|o)");
            if (StringHelper.ContainsNoCase(line1, "box ")
                || StringHelper.ContainsNoCase(line2, "box ")
                || StringHelper.ContainsNoCase(line1, "box#")
                || StringHelper.ContainsNoCase(line2, "box#"))
            {
                if (StringHelper.ContainsNoCase(line1, " box")
                    || StringHelper.ContainsNoCase(line2, " box")
                    || StringHelper.ContainsNoCase(line1, ".box")
                    || StringHelper.ContainsNoCase(line2, ".box"))
                {
                    if (line1.Contains("#")
                        || line2.Contains("#")
                        || line1.Contains("PO")
                        || line2.Contains("PO")
                        || poRegex.IsMatch(line1)//.Contains("P.O")
                        || poRegex.IsMatch(line2))//line2.Contains("P.O")))
                        return true;
                }
            }
            if (line1.StartsWith("box ", StringComparison.InvariantCultureIgnoreCase)) //ex.: Box 6118
                return true;

            return false;
        }


        public static bool HasInvalidSymbols(AddressDTO address)
        {
            var regex = new Regex(@"[^a-zA-Z0-9 -_',./\()!?*]");
            var result = regex.Match(//address.FinalFullName + 
                address.FinalAddress1 
                + address.FinalAddress2
                + address.FinalCity
                + address.FinalCountry
                + address.FinalState
                + address.FinalZip);
            return result.Success;
        }


        public static bool ValidateLenghts(AddressDTO address,
            ShipmentProviderType type,
            long? dropShipperId)
        {
            if (dropShipperId == DSHelper.AshfordDsId)
            {
                //Address length max 30
                if (!String.IsNullOrEmpty(address.Address1) && address.Address1.Length > 30) //Checked
                    return false;
                if (!String.IsNullOrEmpty(address.Address2) && address.Address2.Length > 30) //Checked
                    return false;
            }

            if (type == ShipmentProviderType.IBC)
            {
                /*<ShipTo>
                <Company>40</Company>
                <Attn>40</Attn>
                <AddressLine1>40</AddressLine1>
                <AddressLine2>40</AddressLine2>
                <AddressLine3>40</AddressLine3>
                <City>40</City>
                <StateCode>15</StateCode>
                <Zip>10</Zip>
                <Zip4>10</Zip4>
                <CountryCode>5</CountryCode>
                <CountryName>40</CountryName>
                <PhoneAreaCode>3</PhoneAreaCode>
                <Phone>20</Phone>
                <PhoneExt>10</PhoneExt>
                <Fax>15</Fax>
                <EMail>65</EMail>
                <Department>40</Department>
                <Reference>N/A</Reference>
                <ResidentialFlag>false</ResidentialFlag>
                <IsPOBox>false</IsPOBox>
              </ShipTo>*/

                if (String.IsNullOrEmpty(address.FullName) || address.FullName.Length < 2 || address.FullName.Length > 40)
                    return false;
                if (!String.IsNullOrEmpty(address.Address1) && address.Address1.Length > 40) //Checked
                    return false;
                if (!String.IsNullOrEmpty(address.Address2) && address.Address2.Length > 40) //Checked
                    return false;
                if (!String.IsNullOrEmpty(address.City) && address.City.Length > 40)
                    return false;
                if (!String.IsNullOrEmpty(address.State) && address.State.Length > 15) //Checked
                    return false;
                if (!String.IsNullOrEmpty(address.Zip) && address.Zip.Length > 10)
                    return false;
                if (!String.IsNullOrEmpty(address.Country) && address.Country.Length > 40)
                    return false;
            }

            if (type == ShipmentProviderType.Dhl)
            {
                if (String.IsNullOrEmpty(address.FullName) || address.FullName.Length < 2 || address.FullName.Length > 35)
                    return false;
                if (!String.IsNullOrEmpty(address.Address1) && address.Address1.Length > 35)
                    return false;
                if (!String.IsNullOrEmpty(address.Address2) && address.Address2.Length > 35)
                    return false;
                if (!String.IsNullOrEmpty(address.City) && address.City.Length > 35)
                    return false;
                if (!String.IsNullOrEmpty(address.State) && address.State.Length > 35)
                    return false;
                if (!String.IsNullOrEmpty(address.Zip) && address.Zip.Length > 12)
                    return false;
                if (!String.IsNullOrEmpty(address.Country) && address.Country.Length > 35)
                    return false;
            }
            if (type == ShipmentProviderType.DhlECom)
            {
                if (String.IsNullOrEmpty(address.FullName) || address.FullName.Length < 2 || address.FullName.Length > 30)
                    return false;
                if (!String.IsNullOrEmpty(address.Address1) && address.Address1.Length > 40)
                    return false;
                if (!String.IsNullOrEmpty(address.Address2) && address.Address2.Length > 40)
                    return false;
                if (!String.IsNullOrEmpty(address.City) && address.City.Length > 30)
                    return false;
                if (!String.IsNullOrEmpty(address.State) && address.State.Length > 30)
                    return false;
                if (!String.IsNullOrEmpty(address.Zip) && (address.Zip.Length > 11 || address.Zip.Length < 5))
                    return false;
                if (!String.IsNullOrEmpty(address.Country) && address.Country.Length != 2)
                    return false;
            }
            if (type == ShipmentProviderType.Stamps)
            {
                var isInternational = ShippingUtils.IsInternational(address.FinalCountry);
                if (String.IsNullOrEmpty(address.FullName) || address.FullName.Length < 2)
                    return false;
                if (!String.IsNullOrEmpty(address.Address1) && address.Address1.Length > 50)
                    return false;
                if (!String.IsNullOrEmpty(address.Address2) && address.Address2.Length > 50)
                    return false;
                if (!String.IsNullOrEmpty(address.City) && address.City.Length > 50)
                    return false;
                if (!String.IsNullOrEmpty(address.State) && address.State.Length > 30)
                    return false;
                if (!String.IsNullOrEmpty(address.Zip) && (!isInternational && address.Zip.Length > 5))
                    return false;
                if (!String.IsNullOrEmpty(address.ZipAddon) && (!isInternational && address.ZipAddon.Length > 4))
                    return false;
                if (!String.IsNullOrEmpty(address.Country) && address.Country.Length > 35)
                    return false;
            }
            if (type == ShipmentProviderType.IBC)
            {
                var isInternational = ShippingUtils.IsInternational(address.FinalCountry);
                if (String.IsNullOrEmpty(address.FullName) || address.FullName.Length < 2)
                    return false;
                if (!String.IsNullOrEmpty(address.Address1) && address.Address1.Length > 50)
                    return false;
                if (!String.IsNullOrEmpty(address.Address2) && address.Address2.Length > 50)
                    return false;
                if (!String.IsNullOrEmpty(address.City) && address.City.Length > 50)
                    return false;
                if (!String.IsNullOrEmpty(address.State) && address.State.Length > 30)
                    return false;
                if (!String.IsNullOrEmpty(address.Zip) && (!isInternational && address.Zip.Length > 5))
                    return false;
                if (!String.IsNullOrEmpty(address.ZipAddon) && (!isInternational && address.ZipAddon.Length > 4))
                    return false;
                if (!String.IsNullOrEmpty(address.Country) && address.Country.Length > 35)
                    return false;
            }
            if (type == ShipmentProviderType.Amazon)
            {
                //No validation, do not pass, changes have no effects
            }
            return true;
        }

        public static string PreparePhoneNumber(string phoneNumber)
        {
            if (String.IsNullOrEmpty(phoneNumber))
                return phoneNumber;

            return phoneNumber.Replace("-", "")
                .Replace("/", "")
                .Replace("\\", "")
                .Replace(".", "")
                .Replace("(", "")
                .Replace(")", "")
                .Replace(" ", "");
        }

        public static bool IsEmptyManually(AddressDTO address)
        {
            return String.IsNullOrEmpty(address.ManuallyAddress1)
                   && String.IsNullOrEmpty(address.ManuallyAddress2)
                   && String.IsNullOrEmpty(address.ManuallyZip)
                   && String.IsNullOrEmpty(address.ManuallyCity)
                   && String.IsNullOrEmpty(address.ManuallyCountry);
        }

        public static bool IsEmpty(AddressDTO address)
        {
            return String.IsNullOrEmpty(address.Address1)
                   && String.IsNullOrEmpty(address.Address2)
                   && String.IsNullOrEmpty(address.Zip)
                   && String.IsNullOrEmpty(address.City)
                   && String.IsNullOrEmpty(address.Country);
        }

        public static bool CompareWithManuallyAddressFields(AddressDTO source, AddressDTO address)
        {
            if (source.IsManuallyUpdated)
                return source.ManuallyAddress1 != address.ManuallyAddress1
                   || source.ManuallyAddress2 != address.ManuallyAddress2
                   || source.ManuallyCity != address.ManuallyCity
                   || source.ManuallyState != address.ManuallyState
                   || source.ManuallyZip != address.ManuallyZip
                   || source.ManuallyZipAddon != address.ManuallyZipAddon
                   || source.ManuallyCountry != address.ManuallyCountry;

            return source.Address1 != address.ManuallyAddress1
                   || source.Address2 != address.ManuallyAddress2
                   || source.City != address.ManuallyCity
                   || source.State != address.ManuallyState
                   || source.Zip != address.ManuallyZip
                   || source.ZipAddon != address.ManuallyZipAddon
                   || source.Country != address.ManuallyCountry;
        }

        public static bool CompareWithManuallyAllFields(AddressDTO source, AddressDTO address)
        {
            if (source.IsManuallyUpdated)
                return source.ManuallyAddress1 != address.ManuallyAddress1
                   || source.ManuallyAddress2 != address.ManuallyAddress2
                   || source.ManuallyCity != address.ManuallyCity
                   || source.ManuallyState != address.ManuallyState
                   || source.ManuallyZip != address.ManuallyZip
                   || source.ManuallyZipAddon != address.ManuallyZipAddon
                   || source.ManuallyCountry != address.ManuallyCountry
                   || source.ManuallyPhone != address.ManuallyPhone
                   || source.ManuallyFullName != address.ManuallyFullName;

            return source.Address1 != address.ManuallyAddress1
                   || source.Address2 != address.ManuallyAddress2
                   || source.City != address.ManuallyCity
                   || source.State != address.ManuallyState
                   || source.Zip != address.ManuallyZip
                   || source.ZipAddon != address.ManuallyZipAddon
                   || source.Country != address.ManuallyCountry
                   || source.Phone != address.ManuallyPhone
                   || source.FullName != address.ManuallyFullName;
        }

        public static bool CompareWithManuallyBigChanges(AddressDTO source, AddressDTO address)
        {
            if (source.IsManuallyUpdated)
                return source.ManuallyState != address.ManuallyState
                   || source.ManuallyCountry != address.ManuallyCountry;

            return source.State != address.ManuallyState
                   || source.Country != address.ManuallyCountry;
        }


        public static bool IsFFAddress(AddressDTO address)
        {
            var addressToDismissList = new List<AddressDTO>()
                {
                    new AddressDTO() {Address1 = "1071 NW 31", Zip = "33069"},
                    new AddressDTO() {Address1 = "10813 NW 30TH", Zip = "33172"},
                    new AddressDTO() {Address1 = "18221 150TH", Zip = "11413"},
                    new AddressDTO() {Address1 = "312 CHERRY LN ", Zip = "19720"},
                    new AddressDTO() {Address1 = "397 E 54", Zip = "07407"},
                    new AddressDTO() {Address1 = "3970 NW 132", Zip = "33054"},
                    new AddressDTO() {Address1 = "6703 NW 7", Zip = "33126"},
                    new AddressDTO() {Address1 = "8400 NW 25", Zip = "33198"},
                    new AddressDTO() {Address1 = "1273 CENTRAL FLORIDA PKWY", Zip = "32837"},
                    new AddressDTO() {Address1 = "6085 NW 82ND", Zip = "33166"},
                    new AddressDTO() {Address1 = "8260 NW 14TH ST", Zip = "33126"},
                    new AddressDTO() {Address1 = "8056 NW 66TH ST", Zip = "33195"},
                    new AddressDTO() {Address1 = "182-21 150TH", Zip = "11413"}
                };

            var address1 = address.Address1;
            if (String.IsNullOrEmpty(address1))
                address1 = address.Address2 ?? "";

            if (addressToDismissList.Any(a => address1.Contains(a.Address1) && address.Zip == a.Zip))
            {
                return true;
            }
            return false;
        }


        public static bool IsStampsValidPersonName(string name)
        {
            var parts = name.Split(' ');
            //Enrique F Solivan
            //JP
            if (parts.Count(p => p.Length > 1) >= 2)
                return true;
            return false;
        }

        public static string ToString(AddressDTO address, string separator)
        {
            var searchString = "";
            searchString = StringHelper.JoinTwo(" ", address.FinalAddress1, address.FinalAddress2);
            if (!String.IsNullOrEmpty(address.FinalCity))
                searchString += separator + address.FinalCity;
            if (!String.IsNullOrEmpty(address.FinalState))
                searchString += separator + address.FinalState;

            var zipString = StringHelper.JoinTwo(" ", address.FinalZip, address.FinalZipAddon);
            if (!String.IsNullOrEmpty(zipString))
                searchString += separator + zipString;

            if (!String.IsNullOrEmpty(address.FinalCountry))
                searchString += separator + address.FinalCountry;

            return searchString;
        }

        public static string ToStringForLetterWithPersonName(AddressDTO address)
        {
            var text = address.FinalFullName + "<br/>" +
                address.FinalAddress1 + "<br/>";
            if (!String.IsNullOrEmpty(address.FinalAddress2))
                text += address.FinalAddress2 + "<br/>";

            text += address.FinalCity + ", " +
                    (!String.IsNullOrEmpty(address.FinalState) ? address.FinalState + " " : "")
                    + address.FinalZip + ", " + address.FinalCountry;

            return text;
        }

        public static string GeocodeMessageToDisplay(string message, bool includeOriginalMessage)
        {
            if (String.IsNullOrEmpty(message))
                return message;

            var status = "";
            if (message.IndexOf("Rooftop", StringComparison.OrdinalIgnoreCase) >= 0)
                status = "Good";

            if (message.IndexOf("RangeInterpolated", StringComparison.OrdinalIgnoreCase) >= 0)
                status = "Medium";

            if (message.IndexOf("Approximate", StringComparison.OrdinalIgnoreCase) >= 0 
                || message.IndexOf("GeometricCenter", StringComparison.OrdinalIgnoreCase) >= 0)
                status = "Bad";

            if (!String.IsNullOrEmpty(status))
            {
                if (includeOriginalMessage)
                    return status + " (" + message + ")";
                return status;
            }

            return message;
        }

        public static string GetAddressHash(AddressDTO address)
        {
            return MD5Utils.GetMD5HashAsString(address.FinalCountry
                + "-" + address.FinalCity
                + "-" + address.FinalState
                + "-" + address.FinalZip
                + "-" + address.FinalZipAddon
                + "-" + address.FinalAddress1
                + "-" + address.FinalAddress2);
        }

        public static string GetFirstName(string name)
        {
            if (!String.IsNullOrEmpty(name))
            {
                return name.Split(' ').First();
            }
            return String.Empty;
        }

        public static string GetLastName(string name)
        {
            if (!String.IsNullOrEmpty(name))
            {
                return name.Split(' ').Last();
            }
            return String.Empty;
        }


        public static string CombineZip(string zip, string zipAddon)
        {
            if (String.IsNullOrEmpty(zipAddon))
                return zip;
            return zip + "-" + zipAddon;
        }

        public static int? ZipToInt5(string zip)
        {
            if (String.IsNullOrEmpty(zip))
                return null;

            var zip5 = zip.Replace(" ", "").Split("-".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "";
            zip5 = zip5.Length > 5 ? zip5.Substring(0, 5) : zip5;
            int intZip;
            if (Int32.TryParse(zip5, out intZip))
                return intZip;
            return null;
        }

        public static string GetZip(string zip)
        {
            if (!string.IsNullOrEmpty(zip))
            {
                var l = zip.IndexOf('-');
                return l > 0
                    ? zip.Substring(0, l).Trim().Trim('-')
                    : zip.Trim().Trim('-');
            }
            return zip;
        }

        public static string GetZipAddon(string zip)
        {
            if (!string.IsNullOrEmpty(zip))
            {
                var i = zip.IndexOf('-');
                if (i > 0 && i + 1 <= zip.Length)
                {
                    return zip.Substring(i + 1).Trim().Trim('-');
                }
            }
            return String.Empty;
        }
    }
}
