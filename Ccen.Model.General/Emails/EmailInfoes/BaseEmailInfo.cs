using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Models;
using Amazon.DTO;
using Amazon.Model.Models;

namespace Amazon.Model.Implementation.Emails.EmailInfos
{
    public abstract class BaseEmailInfo
    {
        public abstract EmailTypes EmailType { get; }

        public string ByName { get; set; }

        public string Tag { get; set; }
        
        public MarketType Market { get; set; }
        public string MarketplaceId { get; set; }
        public string ShippingCountry { get; set; }

        public string ToName { get; set; }
        public string ToEmail { get; set; }

        public string CcEmail { get; set; }
        public string CcName { get; set; }

        public string BccEmail { get; set; }
        public string BccName { get; set; }

        public MailAddress From { get; set; }


        protected IAddressService _addressService;

        public BaseEmailInfo(IAddressService addressService)
        {
            _addressService = addressService;
        }

        public List<MailAddress> ToList
        {
            get
            {
                if (String.IsNullOrEmpty(ToEmail))
                    return new List<MailAddress>();

                var emails = ToEmail.Split(",; ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                var names = (ToName ?? "").Split(",; ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                var emailAddresses = new List<MailAddress>();
                for (int i = 0; i < emails.Length; i++)
                {
                    emailAddresses.Add(new MailAddress(emails[i], StringHelper.MakeEachWordFirstLetterUpper(names.Length > i ? names[i] : "")));
                }
                return emailAddresses;
            }
        }

        public List<MailAddress> CcList
        {
            get
            {
                if (String.IsNullOrEmpty(CcEmail))
                    return new List<MailAddress>();

                var emails = CcEmail.Split(",; ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                var names = (CcName ?? "").Split(",; ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                var emailAddresses = new List<MailAddress>();
                for (int i = 0; i < emails.Length; i++)
                {
                    emailAddresses.Add(new MailAddress(emails[i], names.Length > i ? names[i] : ""));
                }
                return emailAddresses;
            }
        }

        public List<MailAddress> BccList
        {
            get
            {
                if (String.IsNullOrEmpty(BccEmail))
                    return new List<MailAddress>();

                var emails = BccEmail.Split(",; ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                var names = (BccName ?? "").Split(",; ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                var emailAddresses = new List<MailAddress>();
                for (int i = 0; i < emails.Length; i++)
                {
                    emailAddresses.Add(new MailAddress(emails[i], names.Length > i ? names[i] : ""));
                }
                return emailAddresses;
            }
        }

        protected IList<Attachment> _attachments = new List<Attachment>(); 
        public IList<Attachment> Attachments
        {
            get { return _attachments; }
        }

        public string BuyerFirstName
        {
            get
            {
                return EmailInfoHelper.GetFirstName(ToName);
            }
        }

        public string BuyerLastName
        {
            get
            {
                return EmailInfoHelper.GetLastName(ToName);
            }
        }

        public string Signature
        {
            get
            {
                var name = StringHelper.MakeEachWordFirstLetterUpper(ByName);

                return String.Format(@"{0}{1}",
                    String.IsNullOrEmpty(name) ? "" : (name + "<br/>"),
                    SignatureCompany);
            }
        }

        public virtual string SignatureCompany
        {
            get { return _addressService == null ? "" : _addressService.GetEmailSignature(Market); }
        }
    }
}
