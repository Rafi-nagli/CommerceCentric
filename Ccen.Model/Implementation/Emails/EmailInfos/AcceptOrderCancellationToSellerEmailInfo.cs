using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Reflection.Emit;
using System.Text;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.Model.Implementation.Emails.EmailInfos;
using Amazon.Model.Implementation.Markets;

namespace Amazon.Model.Models.EmailInfos
{
    public class AcceptOrderCancellationToSellerEmailInfo : BaseEmailInfo, IEmailInfo
    {
        public override EmailTypes EmailType
        {
            get { return EmailTypes.AcceptOrderCancellationToSeller; }
        }
       
        public string SellerUrl
        {
            get
            {
                return MarketUrlHelper.GetSellarCentralOrderUrl(Market, MarketplaceId, Tag, null);
            }
        }

        public string Subject
        {
            get { return String.Format("[System notification] Order cancelled (Order: {0})", OrderHelper.FormatOrderNumber(Tag, Market)); }
        }

        public string Body
        {
            get
            {
                return string.Format(@"<div style=""font-size: 11pt; font-family: Calibri"">
                                <p>Order <a href='{1}'>{0}</a> was canceled per buyer request.</p>
                                <p><i>>> {2}</i></p>
                                <br/>
                                <p>System Notification</p></div>", 
                                                                 Tag,
                                                                 SellerUrl,
                                                                 SourceMessage);
            }
        }

        private string SourceMessage { get; set; }

        public AcceptOrderCancellationToSellerEmailInfo(IAddressService addressService,
            string byName,
            string orderNumber,
            MarketType market,
            string marketplaceId,
            string sourceMessage,
            string sellerName,
            string sellerEmail) : base(addressService)
        {
            ByName = byName;
            Tag = orderNumber;
            Market = market;
            MarketplaceId = marketplaceId;

            SourceMessage = sourceMessage;

            ToName = sellerName;
            ToEmail = sellerEmail;
        }
    }
}
