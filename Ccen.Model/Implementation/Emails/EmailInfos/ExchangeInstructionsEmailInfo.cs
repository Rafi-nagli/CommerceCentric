using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.DTO;
using Amazon.Model.Implementation.Emails.EmailInfos;

namespace Amazon.Model.Models
{
    public class ExchangeInstructionsEmailInfo : BaseEmailInfo, IEmailInfo
    {
        public override EmailTypes EmailType
        {
            get { return EmailTypes.ExchangeInstructions; }
        }

        /*
         * Exchange instructions:
Dear Customer,
We will be happy to exchange pajama you have ordered to size %Size%. 
 
Please send pajama you would like to exchange to:
Premium Apparel 
P.O. Box 398
Hallandale, FL, 33008
 
Include a note or packing slip with your order number.
If you would like your exchanged item to be shipped sooner, please email us tracking number of your return as soon as you have it.
Please Note: the pajama you are returning, should have all the tags attached. 
If you are using carrier other than USPS, please contact us for physical location address.
 
Best Regards,
Premium Apparel
        */

        /* UPDATES 01/06/2017
         * Dear ..,
        We will be happy to exchange %Pajama_Name%,  %Size% (%Color%) to …%New_Pajama_Name%(%New_Color%) %New_Size%

                    %New_Pajama_Name% - needed if different from %Pajama_Name% (or different color)
            */

        public string Subject
        {
            get
            {
                return string.Format("Exchange instructions (Order: {0})", OrderHelper.FormatOrderNumber(Tag, Market));
            }
        }

        public string Body
        {
            get
            {
                var isInternational = ShippingUtils.IsInternational(ShippingCountry);
                var isCanada = (MarketplaceId == MarketplaceKeeper.AmazonCaMarketplaceId || Market == MarketType.WalmartCA);
                var address = _addressService.ReturnAddress;
                var returnAddress = SignatureCompany + String.Format("<br/>{0}{1}<br/>{2}, {3}, {4}",
                    address?.Address1,
                    address?.Address2,
                    address?.City,
                    address?.State,
                    address?.Zip);
                if (isCanada)
                {
                    var caReturnAddress = _addressService.GetReturnAddressByType(CompanyAddressTypes.Canada);
                    returnAddress = String.Format("<br/>{1},<br/>{2},<br/>{3}, {4} {5}<br/>{5}",
                        caReturnAddress.FullName,
                        caReturnAddress.Address1,
                        caReturnAddress.City,
                        caReturnAddress.State,
                        caReturnAddress.Zip,
                        caReturnAddress.Country);
                }
                else
                {
                    if (isInternational)
                    {
                        returnAddress = returnAddress + "<br/>USA";
                    }
                }

                return string.Format(@"<div style=""font-size: 11pt; font-family: Calibri"">
                    <p>Dear {0},<br/>
                    We will be happy to exchange %ExchangeDetails%.</p>
                    <p>Please send product you would like to exchange to:<br/>
                        {2}
                    </p>
                    <p>
                        Include a note or packing slip with your order number.<br/>
                        If you would like your exchanged item to be shipped sooner, please email us tracking number of your return as soon as you have it.<br/>
                        Please Note: the product you are returning, should have all the tags attached.<br/>
                        If you are using carrier other than USPS, please contact us for physical location address.
                    </p>
                    <p>Best Regards,<br/>{1}</p>
                </div>",
                    BuyerFirstName,
                    Signature,
                    returnAddress);
            }
        }

        public ExchangeInstructionsEmailInfo(IAddressService addressService, 
            string byName,
            string orderNumber,
            MarketType market,
            string marketplaceId,
            string buyerName,
            string buyerEmail) : base(addressService)
        {
            ByName = byName;
            Tag = orderNumber;
            Market = market;
            MarketplaceId = marketplaceId;

            ToName = StringHelper.MakeEachWordFirstLetterUpper(buyerName);
            ToEmail = buyerEmail;
        }
    }
}
