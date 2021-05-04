using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Xml;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.Core.Models.Settings;
using Amazon.Core.Models.Validation;
using Amazon.DTO;

namespace Amazon.Model.Implementation.Addresses
{
    //NOTE: docs http://wiki.melissadata.com/images/b/b3/DQT_WS_Personator_RG.pdf
    //with sample REST/XML request/response http://wiki.melissadata.com/images/c/c6/Personator_QSG.pdf

    public class PersonatorAddressCheckService : IAddressCheckService
    {
        private string _customerId = null;
        private ILogService _log;
        private IHtmlScraperService _htmlScraper;

        public AddressProviderType Type
        {
            get { return AddressProviderType.Mellisa; }
        }

        public PersonatorAddressCheckService(ILogService log,
            IHtmlScraperService htmlScraperService,
            string customerId)
        {
            _customerId = customerId;
            _log = log;
            _htmlScraper = htmlScraperService;
        }



        public PersonatorAddressCheckResult ScrappingCheckAddress(AddressDTO address)
        {
            var url = String.Format(
                    "http://www.melissadata.com/Lookups/AddressVerify.asp?name={0}&Company=&Address={1}&city={2}&state={3}&zip={4}",
                    HttpUtility.UrlEncode(address.FinalFullName),
                    HttpUtility.UrlEncode(StringHelper.JoinTwo(" ", address.FinalAddress1, address.FinalAddress2)),
                    HttpUtility.UrlEncode(address.FinalCity),
                    HttpUtility.UrlEncode(address.FinalState),
                    HttpUtility.UrlEncode(StringHelper.JoinTwo("-", address.Zip, address.ZipAddon)));
            

            _log.Info("Request url: " + url);
            var response = _htmlScraper.GetHtml(url, ProxyUseTypes.Mellisa, (proxy, status, html) =>
            {
                var correctContent = !(html ?? "").Contains("Lookup Level Exceeded") && (html ?? "").Contains("Address Verify");
                _log.Info(url + (correctContent ? " (Correct Content)" : " (Incorrect Content)") + ", Status=" + status + ", Is Empty Html=" + String.IsNullOrEmpty(html) +  ", Proxy=" + proxy.IPAddress + ":" + proxy.Port);
                if (status == HttpStatusCode.OK && !correctContent)
                    _log.Info("Content=" + html);

                return status == HttpStatusCode.OK && correctContent && !String.IsNullOrEmpty(html);
            });

            if (response.IsFail)
            {
                _log.Fatal("ScrappingCheckAddress. No result: " + response.Message);
            }
            
            return new PersonatorAddressCheckResult()
            {
                IsNotServedByUSPSNote = (response.Data ?? "").Contains("Address is served by FedEx, UPS and NOT the USPS"),
            };
        }

        public CheckResult<AddressDTO> CheckAddress(CallSource callSource, AddressDTO inputAddress)
        {
            var additionalData = new List<string>() { OrderNotifyType.AddressCheckMelissa.ToString() };

            if (ShippingUtils.IsInternational(inputAddress.FinalCountry))
            {
                return new CheckResult<AddressDTO>()
                {
                    IsSuccess = false,
                    AdditionalData = additionalData
                };
            }

            //The Check action will validate the individual input data pieces for validity and correct them if possible.
            //If the data is correctable, additional information will often be appended as well. US and Canada only.
                //Set the Server URL to the form input
            var url = "https://personator.melissadata.net/v3/WEB/ContactVerify/doContactVerify";
            url += "?t="; //TransmissionReference
            url += "&id=" + _customerId;// "114035921";
            url += "&act=Check";
            //-Plus4
            //Returns the 4-digit plus4 for the input address. If this column is requested, the PostalCode field will
            //only contain the 5-digit ZIP for U.S. addresses.
            //-PrivateMailBox
            //Returns the private mail box number for the address in the AddressLine field, if any. Private mailboxes
            //are private mail boxes in commercial mail receiving agencies, like a UPS Store. If requested, the Private
            //mailbox will be populated in this field instead of the Address field.
            //-Suite
            //Returns the suite for the address in the AddressLine field, if any. If requested, the suite will be
            //populated in this field instead of the Address field.
            url += "&cols=Plus4"; //Columns "Plus4,PrivateMailBox,Suite";
            url += "&opt="; //Options
            url += "&first="; 
            url += "&last=";
            url += "&full=" + HttpUtility.UrlEncode(inputAddress.FinalFullName);
            url += "&comp=";
            url += "&a1=" + HttpUtility.UrlEncode(inputAddress.FinalAddress1);
            url += "&a2=" + HttpUtility.UrlEncode(inputAddress.FinalAddress2);
            url += "&city=" + HttpUtility.UrlEncode(inputAddress.FinalCity);
            url += "&state=" + HttpUtility.UrlEncode(inputAddress.FinalState);
            url += "&postal=" + HttpUtility.UrlEncode(inputAddress.FinalZip 
                + (!String.IsNullOrEmpty(inputAddress.FinalZipAddon) ? "-" + inputAddress.FinalZipAddon : ""));
            url += "&ctry=" + HttpUtility.UrlEncode(inputAddress.FinalCountry);
            url += "&lastlines=";
            url += "&freeform=";
            url += "&email=";
            url += "&phone=" + HttpUtility.UrlEncode(inputAddress.FinalPhone);
            url += "&reserved=";

            //url = url.Replace("#", "%23");
            var uri = new Uri(url);

            CheckResult<AddressDTO> result = null;
            try
            {

                var request = (HttpWebRequest) WebRequest.Create(uri);
                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    using (var responseStream = response.GetResponseStream())
                    {
                        var doc = new XmlDocument();
                        doc.Load(responseStream);

                        result = ProcessResult(doc);
                        result.AdditionalData = additionalData;
                    }
                }
            }
            catch (Exception ex)
            {
                result = new CheckResult<AddressDTO>()
                {
                    IsSuccess = false,
                    Message = ex.Message,
                    AdditionalData = additionalData
                };
            }

            if (result.Data != null)
            {
                var outputAddress = result.Data;
                if (!String.IsNullOrEmpty(outputAddress.AddressKey))
                {
                    if (inputAddress.Address1 == outputAddress.Address1
                        && inputAddress.Address2 == outputAddress.Address2
                        && inputAddress.City == outputAddress.City
                        && inputAddress.Country == outputAddress.Country
                        && inputAddress.State == outputAddress.State
                        && inputAddress.Zip == outputAddress.Zip
                        && inputAddress.ZipAddon == outputAddress.ZipAddon)
                    {
                        result.Data = null;
                    }
                }
                else
                {
                    result.Data = null;
                }
            }

            return result;
        }

        private Dictionary<string, string[]> _codeMessage = new Dictionary<string, string[]>()
        {
            { "AS01", new[] { "Address Fully Verified", "The address is valid and deliverable according to official postal agencies." } },
            { "AS02", new[] { "Street Only Match", "The street address was verified but the suite number is missing or invalid."} },
            { "AS03", new[] {"Non USPS Address Match", "US Only. This US address is not serviced by the USPS but does exist and may receive mail through third party carriers like UPS."} },
            //{ "AS01", "Address matched to postal database" },
            //{ "AS02", "Address matched to non-postal database" },
            //{ "AS03", "Address not deliverable by USPS, but exists" },

            { "AS09", new[] {"Foreign Address", "The address is in a non-supported country"} },
            { "AS10", new[] {"CMRA Address", "US Only. The address is a Commercial Mail Receiving Agency (CMRA) like a Mailboxes Ect. These addresses include a Private Mail Box (PMB or #) number"} },
            { "AS13", new[] {"Address Updated By LACS", "US Only. The address has been converted by LACSLink® from a rural-style address to a city-style address."}},
            { "AS14", new[] {"Suite Appended", "US Only. A suite was appended by SuiteLink™ using the address and company name"} },
            { "AS15", new[] {"Apartment Appended", "An apartment number was appended by AddressPlus using the address and last name."} },
            { "AS16", new[] {"Vacant Address", "US Only. The address has been unoccupied for more than 90 days"} },
            { "AS17", new[] {"No Mail Delivery", "US Only. The address does not currently receive mail but will likely in the near future."} },
            { "AS20", new[] {"Deliverable only by USPS", "US Only. This address can only receive mail delivered through the USPS (ie. PO Box or a military address)."}},
            { "AS23", new[] {"Extraneous Information", "Extraneous information not used in verifying the address was found. This includes unnecessary sub premises and other unrecognized data. The unrecognized data has been placed in the ParsedGarbage field."}},
            

            { "AE01", new[] {"Postal Code Error", "The Postal Code does not exist and could not be determined by the city/municipality and state/province." } },
            { "AE02", new[] {"Unknown Street", "Could not match the input street to a unique street name. Either no matches or too many matches found." } },
            { "AE03", new[] {"Component Mismatch Error", "The combination of directionals (N, E, SW, etc) and the suffix (AVE, ST, BLVD) is not correct and produced multiple possible matches." } },
            { "AE04", new[] {"Non-Deliverable Address", "US Only. A physical plot exists but is not a deliverable addresses. One example might be a railroad track or river running alongside this street, as they would prevent construction of homes in that location."} },
            { "AE05", new[] {"Address Matched to Multiple Records", "The address was matched to multiple records. There is not enough information available in the address to break the tie between multiple records."} },
            { "AE06", new[] {"Address Matched to Early Warning System", "US Only. This address currently cannot be verified but was identified by the Early Warning System (EWS) as containing new streets that might be confused with other existing streets."} },
            { "AE07", new[] {"Missing Minimum Address", "Minimum requirements for the address to be verified is not met. Address must have at least one address line and also the postal code or the locality/administrative area."} },
            { "AE08", new[] {"Suite Not Valid", "The thoroughfare (street address) was found but the sub premise (suite) was not valid."} },
            { "AE09", new[] {"Suite was Missing", "The thoroughfare (street address) was found but the sub premise (suite) was missing."} },
            { "AE10", new[] {"The Premise (house or building) Number Error", "The premise (house or building) number for the address is not valid."} },
            { "AE11", new[] {"The Premise (house or building) Mumber Missing", "The premise (house or building) number for the address is missing."} },
            { "AE12", new[] {"Box Number is Invalid from PO Box or RR", "The PO (Post Office Box), RR (Rural Route), or HC (Highway Contract) Box numer is invalid."} },
            { "AE13", new[] {"PO Box Number Missing from PO Box or RR", "The PO (Post Office Box), RR (Rural Route), or HC (Highway Contract) Box number is missing."} },
            { "AE14", new[] {"Private Mail Box Number Missing", "US Only. The address is a Commercial Mail Receiving Agency (CMRA) and the Private Mail Box (PMB or #) number is missing."} },
            { "AE17", new [] {"Sub Premise (suite) Not Required", "A sub premise (suite) number was entered but the address does not have secondaries."} },


            //{ "AS01", "Address matched to postal database" },
            //{ "AS02", "Address matched to non-postal database" },
            //{ "AS03", "Address not deliverable by USPS, but exists" },
            //{ "AS09", "Foreign Postal Code Detected" },
            //{ "AS10", "Address matched to CMRA" },
            //{ "AS11", "Address Vacant" },
            //{ "AS12", "Address updated by the Move action" },
            //{ "AS13", "Address Updated By LACS" },
            //{ "AS14", "Suite Appended by Suite Link" },
            //{ "AS15", "Address Updated By AddressPlus" },


            { "SE01", new [] { "Web Service Internal Error", "The web service experienced an internal error." }},
            { "GE01", new [] { "Empty XML Request Structure", "The SOAP, JSON, or XML request structure is empty" }},
            { "GE02", new [] { "Empty XML Request Record Structure", "The SOAP, JSON, or XML request record structure is empty" }},
            { "GE03", new [] { "Counted records send more than number of records allowed per request", "The counted records sent more than the number of records allowed per request." }},
            { "GE04", new [] { "CustomerID empty", "The CustomerID is empty" }},
            { "GE05", new [] { "CustomerID not valid", "The CustomerID is invalid" }},
            { "GE06", new [] { "CustomerID disabled", "The CustomerID is disabled." }},
            { "GE07", new [] { "XML Request invalid", "The SOAP, JSON, or XML request is invalid" }},
            { "GE08", new [] { "Invalid CustomerID for Product", "The CustomerID is invalid for this product."}},
            { "GE20", new [] { "Verify package not activated", "The Verify package was requested but is not active for the Customer ID" }},
            { "GE21", new [] { "Append package not activated", "The Append package was requested but is not active for the Customer ID" }},
            { "GE22", new [] { "Move package not activated", "The Move package was requested but is not active for the Customer ID" }},
            { "GW01", new [] { "The license will expire within 2 weeks", "The license will expire within 2 weeks." }}
        };

        public CheckResult<AddressDTO> ProcessResult(XmlDocument doc)
        {
            var outputAddress = new AddressDTO();
            var results = new List<KeyValuePair<string, string>>();

            if (doc.GetElementsByTagName("TransmissionResults").Item(0).HasChildNodes)
            {
                results.AddRange(ParseCodeResult(doc.GetElementsByTagName("TransmissionResults").Item(0).FirstChild.Value));
            }

            if (doc.GetElementsByTagName("Results").Count > 0
                && doc.GetElementsByTagName("Results").Item(0).ChildNodes.Count > 0)
            {
                results.AddRange(ParseCodeResult(doc.GetElementsByTagName("Results").Item(0).FirstChild.Value));
            }
            else
            {
                return new CheckResult<AddressDTO>() { IsSuccess = false };
            }
            
            //Address:
            //Good: AS01, or AS02, or AS03
            //Bad: Everything else.
            //Description: All records with a valid or correctable address are good
            var goodCode = new string[] {"AS01", "AS02", "AS03"};
            var isSuccess = results.All(r => goodCode.Contains(r.Key)); //Had bad code

            outputAddress = new AddressDTO();
            outputAddress.AddressKey = GetFirstChildFor(doc, "AddressKey");
            outputAddress.Country = "US";
            outputAddress.City = GetFirstChildFor(doc, "City");
            outputAddress.State = GetFirstChildFor(doc, "State");
            outputAddress.Address1 = GetFirstChildFor(doc, "AddressLine1");
            outputAddress.Address2 = GetFirstChildFor(doc, "AddressLine2");
            outputAddress.Phone = GetFirstChildFor(doc, "Phone");
            outputAddress.FullName = GetFirstChildFor(doc, "NameFull");
            outputAddress.Zip = GetFirstChildFor(doc, "PostalCode");
            outputAddress.ZipAddon = GetFirstChildFor(doc, "Plus4");

            return new CheckResult<AddressDTO>()
            {
                IsSuccess = isSuccess,
                Message = String.Join("; ", results.Select(r => "[" + r.Key + "]" + (!String.IsNullOrEmpty(r.Value) ? (": " + r.Value) : String.Empty))),
                Data = outputAddress
            };
        }

        private string GetFirstChildFor(XmlDocument doc, string name)
        {
            if (doc.GetElementsByTagName(name).Count > 0)
                if (doc.GetElementsByTagName(name).Item(0).HasChildNodes)
                    return doc.GetElementsByTagName(name).Item(0).FirstChild.Value;
            return null;
        }

        private IList<KeyValuePair<string, string>> ParseCodeResult(string errorCodeString)
        {
            //handle Different Error Codes and Status Codes in each Record
            var messages = new List<KeyValuePair<string, string>>();

            var errorCodeList = errorCodeString.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            var skipCodes = new[] { "NS", "NE" }; //Check (Name) Result Codes
            foreach (var errorCode in errorCodeList)
            {
                var skip = skipCodes.Any(c => errorCode.StartsWith(c));
                if (!skip)
                {
                    if (_codeMessage.ContainsKey(errorCode))
                        messages.Add(new KeyValuePair<string, string>(errorCode, _codeMessage[errorCode][0]));
                    else
                        messages.Add(new KeyValuePair<string, string>(errorCode, string.Empty));
                }
            }

            return messages;
        }
    }
}
