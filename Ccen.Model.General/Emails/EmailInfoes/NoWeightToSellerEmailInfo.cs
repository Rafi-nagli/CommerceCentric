using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Reflection.Emit;
using System.Text;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.DTO;
using Amazon.Model.Implementation.Emails.EmailInfos;

namespace Amazon.Model.Models.EmailInfos
{
    public class NoWeightToSellerEmailInfo : BaseEmailInfo, IEmailInfo
    {
        public override EmailTypes EmailType
        {
            get { return EmailTypes.NoWeightToSeller; }
        }
        
        public IList<string> MissingWeightSizes { get; set; }
        public IList<StyleLocationDTO> Locations { get; set; }

        private string LocationString
        {
            get
            {
                if (Locations == null || !Locations.Any())
                    return "no info";
                return String.Join("<br/>",
                    Locations.Select(
                        l => l.Isle + "/" + l.Section + "/" + l.Shelf + (l.IsDefault ? " (def)" : "")));
            }
        }

        public string Subject
        {
            get { return String.Format("[commercentric.com] Missing weights for {0}", Tag); }
        }

        public string Body
        {
            get
            {
                return string.Format(@"<div style=""font-size: 11pt; font-family: Calibri"">
                                <p>Please provide weights for following sizes:<br/>
                                {0}<br/>Locations:<br/>{1}</p></div>", 
                                                 String.Join("<br/>", MissingWeightSizes),
                                                 LocationString);
            }
        }

        public NoWeightToSellerEmailInfo(IAddressService addressService, 
            string styleString,
            IList<string> missingWeightSizes,
            IList<StyleLocationDTO> locations,
            string toName,
            string toEmail,
            string ccName,
            string ccEmail) : base(addressService)
        {
            Tag = styleString;
            MissingWeightSizes = missingWeightSizes;
            Locations = locations;

            ToName = StringHelper.MakeEachWordFirstLetterUpper(toName);
            ToEmail = toEmail;
            CcName = ccName;
            CcEmail = ccEmail;
        }
    }
}
