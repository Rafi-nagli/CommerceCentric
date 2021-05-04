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
    public class ReturnInstructionsEmailInfo : BaseEmailInfo, IEmailInfo
    {
        public override EmailTypes EmailType
        {
            get { return EmailTypes.ReturnInstructions; }
        }


        /*
         * Return Instructions:
Dear Customer,
We are sorry you have decided to return your order.
You can return it by sending it to
Premium Apparel
P.O. Box 398
Hallandale, FL, 33008
 
Please include a note or packing slip with your order number.
We will issue a refund once we receive it.
We hope to see you again in our store at 
Please Note: the pajama you are returning, should have all the tags attached. 
Provided above address can only be used with USPS (Postal service), if you plan to ship your order back with another shipping carrier (i.e. Fedex or UPS), please contact us for instructions.
 
Best Regards,
Premium Apparel

        */

        public string Subject
        {
            get
            {
                return string.Format("Return Instructions (Order: {0})", OrderHelper.FormatOrderNumber(Tag, Market));
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
                var postalService = isInternational ? "" : "";

                return string.Format(@"<div style=""font-size: 11pt; font-family: Calibri"">
                                <p>Dear {0},<br/>
                                    We are sorry you have decided to return your order.<br/>
                                    You can return it by sending it to<br/>
                                    {3}
                                </p>
                                <p>
                                    Please include a note or packing slip with your order number.<br/>
                                    We will issue a refund once we receive it.<br/>
                                    We hope to see you again in our store at {2}<br/>
                                    Please Note: the product you are returning, should have all the tags attached.
                                    {4}
                                </p>
                                <p>Best Regards,<br/>{1}</p>
                            </div>",
                    BuyerFirstName,
                    Signature,
                    StoreLink,
                    returnAddress,
                    postalService);
            }
        }

        public string StoreLink
        {
            get
            {
                if (Market == MarketType.Amazon
                    || Market == MarketType.AmazonEU
                    || Market == MarketType.AmazonAU)
                    return "<a href='www.amazon.com/shops/premiumapparel'>www.amazon.com/shops/premiumapparel</a>";
                if (Market == MarketType.Walmart)
                    return "<a href='https://www.walmart.com/search/?facet=retailer:Premium+Apparel'>https://www.walmart.com/search/?facet=retailer:Premium+Apparel</a>";
                if (Market == MarketType.WalmartCA)
                    return "<a href='https://www.walmart.ca/search/?facet=retailer:Premium+Apparel'>https://www.walmart.ca/search/?facet=retailer:Premium+Apparel</a>";
                return "";
            }
        }

        public ReturnInstructionsEmailInfo(IAddressService addressService, 
            string byName,
            string orderNumber,
            MarketType market,
            string marketplaceId,
            string buyerName,
            string buyerEmail,
            string shippingCountry) : base(addressService)
        {
            ByName = byName;
            Tag = orderNumber;
            Market = market;
            MarketplaceId = marketplaceId;
            ShippingCountry = shippingCountry;

            ToName = StringHelper.MakeEachWordFirstLetterUpper(buyerName);
            ToEmail = buyerEmail;
        }
    }
}
