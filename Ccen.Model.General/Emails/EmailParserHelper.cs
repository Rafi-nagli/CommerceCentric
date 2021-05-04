using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using Amazon.Common.Helpers;
using Amazon.Core.Models;
using Amazon.DTO;

namespace Amazon.Model.Implementation.Emails
{
    public class EmailParserHelper
    {
        //ex.: 108-5975746-8479448
        private const string ID_AMAZON_REGEX = @"(\d+){3}-(\d+){7}-(\d+){7}";

        //Walmart CA: Order ID = 2282223729
        //Order Y10181908
        private const string ID_WALMART_CA_REGEX = @"((Y)?[\d- ]{8,19})";

        private const string ID_WALMART_REGEX = @"([\d- ]{12,19})";

        private const string TRANSACTION_ID_EBAY_REGEX = @"Transaction ID:\s*((\d+){12,15})";
        //Item Id: 263007999482
        private const string ITEM_ID_EBAY_REGEX = @"Item ID:\s*((\d+){12,15})";

        public const string ID_UNIVERSAL_REGEX = @"([\d- ]{12,24})";

        private static List<string> _timeZones = new List<string>
                                                       {
                                                           "ACDT", "ACST", "ADT", "AEDT", "AEST", "AHDT", "AHST",
                                                           "AST", "AT", "AWDT", "AWST", "BAT", "BDST", "BET", "BST", 
                                                           "BT", "BZT2", "CADT", "CAST", "CAT", "CCT", "CDT", "CED",
                                                           "CET", "CST", "CENTRA", "EAST", "EDT", "EED", "EET", "EEST",
                                                           "EST", "EASTER", "FST", "FWT", "GMT", "GST", "HDT", "HST",
                                                           "IDLE", "IDLW", "IST", "IT", "JST", "JT", "MDT", "MED",
                                                           "MET", "MEST", "MEWT", "MST", "MOUNTA", "MT", "NDT", "NFT",
                                                           "NT", "NST", "NZ", "NZST", "NZDT", "NZT", "PDT", "PST",
                                                           "PACIFI", "ROK", "SAD", "SAST", "SAT", "SDT", "SST", "SWT",
                                                           "USZ3", "USZ4", "USZ5", "USZ6", "UT", "UTC", "UZ10", "WAT",
                                                           "WET", "WST", "YDT", "YST", "ZP4", "ZP5", "ZP6"
                                                       };
 

        static public string GetMessageID(NameValueCollection headers)
        {
            if (headers.AllKeys.Contains("Message-ID"))
                return headers["Message-ID"];
            return string.Empty;
        }

        static public string GetMessageLabels(NameValueCollection headers)
        {
            if (headers.AllKeys.Contains("X-GM-LABELS"))
                return headers["X-GM-LABELS"];
            return string.Empty;
        }

        static public EmailDTO ParseEmail(MailMessage mailMessage, 
            uint uid, 
            string folder,
            DateTime scanDate)
        {
            var from = mailMessage.From != null ? mailMessage.From.Address : string.Empty;
            var to = GetAddressesString(mailMessage.To);
            var copyTo = GetAddressesString(mailMessage.CC);
            var receiveDate = GetRecieveDate(mailMessage);
            var isHtml = (mailMessage.Body ?? "").Contains("<html");
            var htmlBody = GetHtmlBodyString(mailMessage);
            if (String.IsNullOrEmpty(htmlBody) && isHtml)
            {
                htmlBody = mailMessage.Body;
            }

            var body = htmlBody ?? mailMessage.Body.Replace("\r\n", "<br />");

            body = StringHelper.Substring(body, EmailHelper.MaxBodyLength);
            
            var email = new EmailDTO
            {
                UID = uid,
                MessageID = GetMessageID(mailMessage.Headers),
                From = from,
                To = to,
                CopyTo = copyTo,
                Subject = mailMessage.Subject,
                Message = body,
                ReceiveDate = receiveDate,
                CreateDate = scanDate,
            };
            return email;
        }

        static public IEnumerable<string> GetEBayAssociatedOrderIds(string emailBody)
        {
            var noHtml = StringHelper.TrimTags(emailBody);

            var transactionMatches = Regex.Matches(noHtml, TRANSACTION_ID_EBAY_REGEX, RegexOptions.IgnoreCase);
            var itemMatches = Regex.Matches(noHtml, ITEM_ID_EBAY_REGEX, RegexOptions.IgnoreCase);

            var transactionIds = new List<string>();
            var itemIds = new List<string>();

            foreach (Match match in transactionMatches)
            {
                var number = StringHelper.RemoveWhitespace(match.Value.Replace("Transaction ID:", ""));
                if (!String.IsNullOrEmpty(number))
                    transactionIds.Add(number);
            }

            foreach (Match match in itemMatches)
            {
                var number = StringHelper.RemoveWhitespace(match.Value.Replace("Item ID:", ""));
                if (!String.IsNullOrEmpty(number))
                    itemIds.Add(number);
            }

            var results = transactionIds;
            if (transactionIds.Count == 1 
                && itemIds.Count == 1)
                results.Add(itemIds[0] + "-" + transactionIds[0]);

            return results;
        }

        static public IEnumerable<string> GetAmazonAssociatedOrderIds(string emailSubject)
        {
            //Amazon
            var matches = Regex.Matches(emailSubject, ID_AMAZON_REGEX, RegexOptions.IgnoreCase);
            var numbers = new List<string>();
            foreach (Match match in matches)
            {
                var number = StringHelper.RemoveWhitespace(match.Value);
                if (!String.IsNullOrEmpty(number))
                    numbers.Add(number);
            }
            
            return numbers;
        }

        static public IEnumerable<string> GetShopifyAssociatedOrderIds(string emailSubject, string regex)
        {
            var matches = Regex.Matches(emailSubject, regex, RegexOptions.IgnoreCase);
            var numbers = new List<string>();
            foreach (Match match in matches)
            {
                var number = StringHelper.RemoveWhitespace(match.Value);
                if (!String.IsNullOrEmpty(number))
                    numbers.Add(number);
            }

            return numbers;
        }

        public static IEnumerable<string> GetWalmartAssociatedOrderIds(string emailSubject)
        {
            var matches = Regex.Matches(emailSubject, ID_WALMART_REGEX, RegexOptions.IgnoreCase);
            var numbers = new List<string>();
            foreach (Match match in matches)
            {
                var number = StringHelper.RemoveWhitespace(match.Value).Replace("-", "");
                if (!String.IsNullOrEmpty(number))
                {
                    //6211660994582 or 3577389576718 or 2573450870828 or 457-745-084-0432 or 157-745-127-2366
                    if (number.Length == 13)
                        numbers.Add(number);
                }
            }

            return numbers;
        }

        public static IEnumerable<string> GetWalmartCAAssociatedOrderIds(string emailSubject)
        {
            var matches = Regex.Matches(emailSubject, ID_WALMART_CA_REGEX, RegexOptions.IgnoreCase);
            var numbers = new List<string>();
            foreach (Match match in matches)
            {
                var number = StringHelper.RemoveWhitespace(match.Value).Replace("-", "");
                if (!String.IsNullOrEmpty(number))
                {
                    if (number.Length > 8)
                        numbers.Add(number);
                }
            }

            return numbers;
        }

        private static DateTime GetRecieveDate(MailMessage email)
        {
            var dateString = email.Headers["Date"];
            var isGmt = !String.IsNullOrEmpty(dateString)
                && (dateString.EndsWith("GMT") || dateString.EndsWith("UTC"));
            dateString = RemoveTimeZone(dateString);
            DateTime receiveDate;
            var parsed = DateTime.TryParse(dateString, out receiveDate);
            if (parsed)
            {
                return TimeZoneInfo.ConvertTimeFromUtc(!isGmt ? receiveDate.ToUniversalTime() : receiveDate, DateHelper.AppTimeZoneInfo);
            }
            return DateHelper.GetAppNowTime().AddHours(-1);
        }

        private static string RemoveTimeZone(string dateString)
        {
            var charTimeZoneIndex = dateString.IndexOf('(');
            if (charTimeZoneIndex > 0)
            {
                dateString = dateString.Substring(0, charTimeZoneIndex);
            }
            else
            {
                foreach (var timeZone in _timeZones)
                {
                    if (!dateString.Contains(timeZone))
                        continue;
                    dateString = dateString.Replace(timeZone, "");
                    break;
                }
            }
            return dateString;
        }

        private static string GetAddressesString(MailAddressCollection addressCollection)
        {
            return String.Join("; ", addressCollection.Select(a => a.Address));
        }

        private static string GetHtmlBodyString(MailMessage mailMessage)
        {
            var htmltext =
                mailMessage.AlternateViews.Where(
                    v => v.ContentType.ToString() == "text/html").Select(
                        v => v.ContentStream).FirstOrDefault();

            if (htmltext != null)
            {
                return new StreamReader(htmltext, mailMessage.BodyEncoding).ReadToEnd();
            }
            return null;
        }
    }
}
