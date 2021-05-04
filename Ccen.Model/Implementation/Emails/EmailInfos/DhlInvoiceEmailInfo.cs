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
    public class DhlInvoiceEmailInfo : BaseEmailInfo, IEmailInfo
    {
        public override EmailTypes EmailType
        {
            get { return EmailTypes.DhlInvoice; }
        }


        /*
         * This is an automated message
Premium Apparel didn't receive an electronic invoice in excel format for the week XXX-XXX (i.e. 2/15/2016-2/22/2016). Please forward it to billing@premiumapparel.com to avoid delays with the payment. For any questions please contact Rafi Nagli at 786-942-7939 or over the email rafi@premiumapparel.com

        */

        public DateTime StartWeek { get; set; }
        public DateTime EndWeek { get; set; }

        public string Subject
        {
            get { return "Missing Invoice"; }
        }

        public string Body
        {
            get
            {
                return string.Format(@"<div style=""font-size: 11pt; font-family: Calibri"">
                                <p><b>This is an automated message</b><br/>
                                   Premium Apparel didn't receive an electronic invoice in excel format for the week {0}-{1}.<br/>
                                   Please forward it to billing@premiumapparel.com to avoid delays with the payment. For any questions please contact Rafi Nagli at 786-942-7939 or over the email <a href='mailto: rafi@premiumapparel.com'>rafi@premiumapparel.com</a>
                                </p>
                            </div>",
                            StartWeek.ToString("MM/dd/yyyy"),
                            EndWeek.ToString("MM/dd/yyyy"));
            }
        }

        public DhlInvoiceEmailInfo(IAddressService addressService, 
            DateTime startWeek,
            DateTime endWeek,
            string toEmail,
            string ccEmail) : base(addressService)
        {
            Tag = "DHL Invoice";

            StartWeek = startWeek;
            EndWeek = endWeek;

            ToEmail = toEmail;
            CcEmail = ccEmail;
        }
    }
}
