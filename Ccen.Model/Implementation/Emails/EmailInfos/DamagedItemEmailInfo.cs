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
    public class DamagedItemEmailInfo : BaseEmailInfo, IEmailInfo
    {
        public override EmailTypes EmailType
        {
            get { return EmailTypes.DamagedItem; }
        }

        public bool IsOnePiece { get; set; }

        /*
         “              
Dear %Name%,
We are sorry for the issue with you order. Can you please indicate which Item is damaged, and attach the picture of the damage?
We are looking forward to hear back from you soon, and will promptly process your claim.

Best Regards,
Customer Service”


        */

        public string Subject
        {
            get
            {
                return string.Format("Damaged Item(s) (Order: {0})", OrderHelper.FormatOrderNumber(Tag, Market));
            }
        }

        public string Body
        {
            get
            {
                if (IsOnePiece)
                {
                    return string.Format(@"<div style=""font-size: 11pt; font-family: Calibri"">
                                <p>Dear {0},<br/>
                                    We are sorry for the issue with your order. Can you please indicate which Item is damaged, and attach the picture of the damage?<br/>
                                    We are looking forward to hearing back from you soon, and will promptly process your claim.
                                </p>
                                <p>Best Regards,<br/>{1}</p>
                            </div>",
                            BuyerFirstName,
                            Signature);
                }
                else
                {
                    return string.Format(@"<div style=""font-size: 11pt; font-family: Calibri"">
                                <p>Dear {0},<br/>
                                    We are sorry for the issue with you order. Can you please indicate which Item is damaged, and attach the picture of the damage?<br/>
                                    We are looking forward to hear back from you soon, and will promptly process your claim.
                                </p>
                                <p>Best Regards,<br/>{1}</p>
                            </div>",
                        BuyerFirstName,
                        Signature);
                }
            }
        }

        public override string SignatureCompany
        {
            get { return "Customer Service"; }
        }

        public DamagedItemEmailInfo(IAddressService addressService, 
            string byName,
            string orderNumber,
            MarketType market,
            bool isOnePiece,
            string buyerName,
            string buyerEmail) : base(addressService)
        {
            ByName = byName;
            Tag = orderNumber;
            Market = market;
            IsOnePiece = isOnePiece;
            ToName = StringHelper.MakeEachWordFirstLetterUpper(buyerName);
            ToEmail = buyerEmail;
        }
    }
}
