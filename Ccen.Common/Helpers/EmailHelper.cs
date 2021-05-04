using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;

namespace Amazon.Common.Helpers
{
    public class EmailHelper
    {
        public static string InboxFolder = "INBOX";
        public static string SentFolder = "[Gmail]/Sent Mail";

        public static string UserNameHeadersKey = "BY-USER-NAME";

        public static string WalmartSupportEmail = "walmart@premiumapparel.com";

        public static string RafiEmail = "rafi@premiumapparel.com";
        public static string SvetaEmail = "sveta@premiumapparel.com";
        public static string LaniEmail = "lani@premiumapparel.com";
        //public static string AshleyEmail = "ashley@premiumapparel.com";
        public static string JenniferEmail = "jennifer@premiumapparel.com";
        public static string TonyEmail = "tfootsies@yahoo.com";
        public static string SupportDgtexEmail = "support@dgtex.com";
        public static string SupportPAEmail = "support@premiumapparel.com";
        public static string IldarDgtexEmail = "ildar@dgtex.com";
        public static string RaananEmail = "raanan@premiumapparel.com";

        //TMX
        public static string TMXTimEmail = "tim@morexinternational.com";

        //ZZG
        public static string OlgaEmail = "happyolga12@gmail.com";
        public static string ZzgEmail = "zizzlinginc@gmail.com";

        //TNR
        public static string TNRNisusEmail = "consumer@mynisus.com";

        //DWS
        public static string MicheleEmail = "michele@3jtrading.com";
        public static string YoniEmail = "yoni@gsdglobalinc.com";

        public static string DhlInvoiceRecipient = "maryruth.peeke@inxpress.com";

        public static bool ResponseNeeded(string answerMessageID,
            int emailType,
            int responseStatus,
            int folderType)
        {
            return answerMessageID == null
                   && emailType != (int) IncomeEmailTypes.SystemAutoCopy
                   && emailType != (int) IncomeEmailTypes.AZClaim
                   && (responseStatus == (int) EmailResponseStatusEnum.None
                       || responseStatus == (int) EmailResponseStatusEnum.NoResponseNeeded)
                   && folderType == (int) EmailFolders.Inbox;
        }

        public static string[] StringToEmails(string text)
        {
            return (text ?? "").Split(";, ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        }

        public static EmailFolders GetFolderType(string folder)
        {
            if (String.IsNullOrEmpty(folder))
                return EmailFolders.Inbox;

            var folderType = EmailFolders.Inbox;
            if (folder.IndexOf("sent", StringComparison.OrdinalIgnoreCase) >= 0)
                folderType = EmailFolders.Sent;

            return folderType;
        }


        public static string PrepareReplySubject(string subject)
        {
            if (String.IsNullOrEmpty(subject))
                return "RE:";
            if (subject.StartsWith("re:", StringComparison.OrdinalIgnoreCase))
                return subject;
            return "RE: " + subject;
        }

        public static string GetWoReplySubject(string subject)
        {
            if (String.IsNullOrEmpty(subject))
                return subject;

            return StringHelper.ReplaceString(subject, "RE:", "", StringComparison.OrdinalIgnoreCase).TrimStart();
        }

        public static bool IsAutoCommunicationAddress(string address)
        {
            if (String.IsNullOrEmpty(address))
                return false;

            if (address.Contains("auto-communication@amazon"))
                return true;

            return false;
        }

        public static string ExtractWalmartItemId(string message)
        {
            if (String.IsNullOrEmpty(message))
                return null;

            /* Example:
             * Hi Mia Belle Girls, 

            Lauren Brady wants to connect with you regarding the following order:

            Sales Order ID:  5921679250664
            Item ID:  162457985
            Item:  Barbie Girls' Selfie 2 Piece Pajama Set, Sizes 4-10

            <div dir="ltr"><br />    <div>Hi Premium Apparel,&nbsp;</div><br />    <div><br></div><br />    <div>Lauren Brady wants to connect with you regarding the following order:</div><br />    <div><br></div><br />    <div>Sales Order ID:&nbsp; 5921679250664</div><br />    <div>Item ID:&nbsp; 162457985</div><br />    <div>Item:&nbsp; Barbie Girls' Selfie 2 Piece Pajama Set, Sizes 4-10</div><br />    <div><br><br></div><br />    <div>Message:<br/></div><br />    I didn't mean to push the order button. Can I cancel this order? I need to do some checking on sizes and was just looking to see what was there. Thank you, Lauren Brady<br />    <div><br></div><br />    <div><i>**Note: You have received this message from the customer, who has made a purchase from your company on<br />        Walmart.com. Please respond within 1 business day.**</i><br />    </div><br />    <div><br><br></div><br />    <div>Thank you,&nbsp;</div><br />    <div>Walmart Customer Service Team</div><br />    <br><br />    <br><br />    <i>We will send a copy of your message to the customer, but we will not include your email address in that copy. We will keep a copy of each email sent and received, and we may review them to resolve disputes, improve customer experience, and assess seller performance. By using this messaging service, you agree to our retention and review of your messages. A copy of this message will be sent to walmart@premiumapparel.com</i><br />    <br><br />    <br><br />    <div>RefID: CFB3AF9BD4F64299A86742C482665D44</div><br /></div><br />
            */

            /* Walmart.ca example:
            ...<div>saravjeet wants to connect with you regarding the following order:</div>      <div><br></div>      <div>Sales Order ID:&nbsp; 2282223729</div>      <div>Item ID:&nbsp; 198664262</div>      <div>Item:&nbsp; Paw Patrol Boys Pajamas (Toddler)</div>      <div><br><br></div>      <div>Message:<br/></div>      Hi- i would like to cancel this order. My order has not yet shipped. Please process the cancellation.      <div><br></div>      <div><i>**Note: You hav...
            */

            if (message.Contains("Item ID:"))
            {
                var keyWord = "Item ID:";
                var beginIndex = message.IndexOf(keyWord, StringComparison.OrdinalIgnoreCase);
                if (beginIndex >= 0)
                {
                    beginIndex += keyWord.Length;
                    var endIndex = message.IndexOfAny("\r\n<".ToCharArray(), beginIndex);
                    if (endIndex > 0)
                    {
                        return StringHelper.TrimWhitespace(message.Substring(beginIndex, endIndex - beginIndex).Replace("&nbsp;", " "));
                    }
                }
            }

            return null;
        }

        public static string ExtractShortSubject(string subject)
        {
            if (String.IsNullOrEmpty(subject))
                return "";

            var index = subject.LastIndexOf("inquiry from Amazon", StringComparison.InvariantCultureIgnoreCase);
            if (index >= 0)
                return "Inquiry";
            
            index = subject.LastIndexOf("from Amazon", StringComparison.InvariantCultureIgnoreCase);

            if (index > 0)
                return subject.Remove(index).Trim();
            
            return subject;
        }

        public static string ExtractShortMessageBody(string message, int length, bool trimEnd)
        {
            //Example 1: (Objet: Demande de renseignements de la part du client Amazon sarah McMillin)<br /><br />Order ID/Commande N°: 701-4152897-2930614:<br />1 of/sur Caillou Toddler Red Poly Pajamas (3T) [ASIN: B00TKJISSS] <br /><br />-------------- Begin message / Début du message -------------<br /><br />Ordered wrong size. Please send 2t.<br /><br />-------------- End message / Fin du message --------------<br /><br />For Your Information: To help arbitrate disputes and preserve trust and safety, we retain all messages buyers and sellers send through Amazon.ca.  This includes your response to the message above.<br /><br />We want you to buy with confidence anytime you purchase products on Amazon.ca. Learn more about Safe Online Shopping (http://www.amazon.ca/gp/help/customer/display.html?nodeId=13940691) and our safe buying guarantee (http://www.amazon.ca/gp/help/customer/display.html?nodeId=200342110).<br /><br />Important : Afin d'aider au règlement des litiges et de protéger la confiance et la sécurité de notre site, nous gardons une copie de tous les courriels envoyés et reçus par le biais d'Amazon.ca, y compris votre réponse au message ci-dessus.<br /><br />Il est important pour nous que vous puissiez à tout moment effectuer vos achats sur Amazon.ca en toute confiance. Pour davantage d'informations concernant la sécurité des transactions, merci de visiter la page (http://www.amazon.ca/gp/help/customer/display.html?nodeId=13940691). Pour en savoir plus sur la protection offerte par notre garantie A-Z, merci de consulter la page (http://www.amazon.ca/gp/help/customer/display.html?nodeId=200342110).<br /><br />If you believe this message is suspicious, please report it to us here: http://www.amazon.ca/gp/communication-manager/report.html?ft=InappropriateContent&msg=AYOWMP58EXOS9&d=1473100072&mp=A2EUQ1WTGCTBG2&v=1&t=038e596eeae79f50af6a239de6b6224a6de17d59<br /><br />Si vous trouvez ce message suspicieux, merci de nous le faire savoir ici : http://www.amazon.ca/gp/communication-manager/report.html?ft=InappropriateContent&msg=AYOWMP58EXOS9&d=1473100072&mp=A2EUQ1WTGCTBG2&v=1&t=038e596eeae79f50af6a239de6b6224a6de17d59<br /><br />To mark this message as no response needed, click here: http://www.amazon.ca/gp/communication-manager/no-response-needed.html?msg=AYOWMP58EXOS9&d=1473100072&mp=A2EUQ1WTGCTBG2&v=1&t=038e596eeae79f50af6a239de6b6224a6de17d59<br /><br />Pour indiquer que ce message ne requiert aucune réponse, cliquez ici : http://www.amazon.ca/gp/communication-manager/no-response-needed.html?msg=AYOWMP58EXOS9&d=1473100072&mp=A2EUQ1WTGCTBG2&v=1&t=038e596eeae79f50af6a239de6b6224a6de17d59<br /><br /><br /><br />[commMgrTok:AYOWMP58EXOS9]<br />
            //Example 2: Yes please, I'd like to exchange it for the 14/16.<br /><br />-----Original Message-----<br />From: Premium Apparel - Amazon Marketplace [mailto:[e-mail address removed]]<br />Sent: Tuesday, September 06, 2016 9:08 AM<br />To: Haught, Laura/WDC <[e-mail address removed]><br />Subject: RE: Exchange request for order (Order: 102-5239268-9612243) [EXTERNAL]<br /><br />Order ID 102-5239268-9612243:<br />1 of Girls' Velvet Plush Cozy Robe in Variety Colors, Owls, Size 7/8 [ASIN: B01FCAWEYM]<br /><br />------------- Begin message -------------<br /><br />Unfortunately we don't have size L (10/12). Please let us know if you like to exchange to 14/16.<br /><br />--- Original message ---<br /><br />Thank you. Do you have a 12/14?<br /><br />From: Premium Apparel - Amazon Marketplace [mailto:[e-mail address removed]]<br />Sent: Tuesday, September 06, 2016 8:25 AM<br />To: Haught, Laura/WDC <[e-mail address removed]><br />Subject: Exchange request for order (Order: 102-5239268-9612243) [EXTERNAL]<br /><br /><br />Dear Mr./Mrs. Haught,<br />Thank you for your order. We are sorry the item you have received didn't fit.<br />We will be happy to exchange it free of charge for bigger size - 14/16.<br />Please let us know if you like us to send you a bigger size.<br /><br />Best Regards,<br />Premium Apparel<br />For Your Information: To help arbitrate disputes and preserve trust and safety, we retain all messages buyers and sellers send through Amazon.com for two years. This includes your response to the message above. Amazon.com uses filtering technology to protect buyers and sellers from possible fraud. Messages that fail this filtering will not be transmitted.<br /><br />We want you to buy with confidence anytime you purchase products on Amazon.com. Learn more about Safe Online Shopping (http://www.amazon.com/gp/help/customer/display.html?nodeId=551434) and our safe buying guarantee (http://www.amazon.com/gp/help/customer/display.html?nodeId=537868).<br /><br /><br /><br /><br />[Image removed by sender.]<br /><br /><br /><br /><br /><br /><br /><br />------------- End message -------------<br /><br />For Your Information: To help arbitrate disputes and preserve trust and safety, we retain all messages buyers and sellers send through Amazon.com for two years.  This includes your response to the message above.  Amazon.com uses filtering technology to protect buyers and sellers from possible fraud.  Messages that fail this filtering will not be transmitted.<br /><br />We want you to buy with confidence anytime you purchase products on Amazon.com. Learn more about Safe Online Shopping (http://www.amazon.com/gp/help/customer/display.html?nodeId=551434) and our safe buying guarantee (http://www.amazon.com/gp/help/customer/display.html?nodeId=537868).<br /><br />If you believe this message is suspicious, please report it to us here: http://www.amazon.com/gp/communication-manager/report.html?ft=InappropriateContent&msg=AF9E104T7W2Q2&d=1473167414&mp=ATVPDKIKX0DER&v=1&t=9361b955ebcbc8373fb5e6f93235a69c0f20364f<br /><br />To mark this message as no response needed, click here: http://www.amazon.com/gp/communication-manager/no-response-needed.html?msg=AF9E104T7W2Q2&d=1473167414&mp=ATVPDKIKX0DER&v=1&t=9361b955ebcbc8373fb5e6f93235a69c0f20364f<br /><br /><br /><br />[commMgrTok:AF9E104T7W2Q2]<br />

            var noHtml = StringHelper.TrimTags(message ?? "");

            var firstLine = StringHelper.Substring(noHtml, 100);

            if (firstLine.Contains("here is a copy of the e-mail that you sent") || firstLine.Contains("Order ID"))
            {
                //Looking "--begin message--" and get 100 characters after it

                var index = StringHelper.SuquenceSearch(noHtml, new string[]
                {
                    "Begin message",
                    "-------"
                });

                if (index > 0)
                {
                    var text = StringHelper.Substring(noHtml, index, length);
                    text = text.Trim(" -\r\n\t".ToCharArray());
                    if (trimEnd)
                        return StringHelper.RemoveAfter(text, new string[] { "------", "--- Original message" });
                    return text;
                }
            }

            /* Sabject: An inquiry from one of our customers 
            Hello,
            We've been contacted by a customer regarding the order identified below.

            --------------------
            Order#: 110-3670889-2424259
            Item: Star Wars Big Boys' Vader 2-Piece Pajama Set, Black, 10 
            Reason: Tracking info

            Details: 
            Hello ,

            Please send updated tracking info for the delivery of the package. Please contact the customer regarding the details.

            Hoping for your immediate response. Thank you and have a good day!*/
            if (firstLine.Contains("We've been contacted by a customer regarding the order identified below.")) //NOTE: result only first 100 symbols
            {
                var index = StringHelper.SuquenceSearch(noHtml, new string[]
                {
                    "------",
                    "Reason:"
                });
                
                if (index > 0)
                {
                    var text = StringHelper.Substring(noHtml, index, length);
                    text = text.Trim(" -\r\n\t".ToCharArray());
                    if (trimEnd)
                        return StringHelper.RemoveAfter(text, "------");
                    return text;
                }
            }

            //NOTE: Walmart
            if (firstLine.Contains("with you regarding the following order"))
            {
                var index = StringHelper.SuquenceSearch(noHtml, new string[]
                {
                    "Sales Order ID:",
                    "Message:"
                });

                if (index > 0)
                {
                    var text = StringHelper.Substring(noHtml, index, length);
                    return text;
                }
            }

            return StringHelper.Substring(noHtml, length);
        }

        public static int MaxBodyLength = 65535;

        public static string CutOutBody(string sourceBody, out bool wasModified)
        {
            wasModified = false;

            if (String.IsNullOrEmpty(sourceBody))
                return sourceBody;

            var beginIndex = sourceBody.IndexOf(@"<div id=""amznCommMgrFooter""");
            if (beginIndex > 0)
            {
                var endIndex = sourceBody.IndexOf(@"</body>", beginIndex);
                if (endIndex > 0)
                {
                    sourceBody = sourceBody.Remove(beginIndex, endIndex - beginIndex);
                    wasModified = true;
                }
            }
            beginIndex = sourceBody.IndexOf("For Your Information: To help arbitrate ");
            if (beginIndex > 0)
            {
                var endKeyword = "BBC_MESSAGE_SENT_TO_MERCHANT";
                var addonLength = 20;
                var endIndex = sourceBody.IndexOf(endKeyword, beginIndex);
                if (endIndex == -1)
                {
                    endKeyword = "[commMgrTok:";
                    endIndex = sourceBody.IndexOf(endKeyword);
                    if (endIndex > 0)
                    {
                        endIndex = sourceBody.IndexOf("]", endIndex);
                    }
                }

                if (endIndex > 0)
                {
                    if (endIndex + endKeyword.Length + 20 > sourceBody.Length)
                    {
                        sourceBody = sourceBody.Remove(beginIndex);
                        wasModified = true;
                    }
                }
            }

            if (sourceBody.Length > MaxBodyLength)
            {
                sourceBody = sourceBody.Substring(0, MaxBodyLength);
                wasModified = true;
            }

            return sourceBody;
        }
    }
}
