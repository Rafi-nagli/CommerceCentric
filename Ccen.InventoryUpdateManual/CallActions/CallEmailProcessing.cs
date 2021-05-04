using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Contracts.SystemActions;
using Amazon.Core.Models;
using Amazon.Core.Models.SystemActions.SystemActionDatas;
using Amazon.DTO;
using Amazon.DTO.Users;
using Amazon.InventoryUpdateManual.Models;
using Amazon.Model.Implementation;
using Amazon.Model.Implementation.Emails;
using Amazon.Model.Implementation.Emails.Rules;
using Amazon.Model.Models;
using Amazon.Model.SyncService.Threads.Simple.Notifications;
using Moq;

namespace Amazon.InventoryUpdateManual.CallActions
{
    public class CallEmailProcessing
    {
        private ILogService _log;
        private ITime _time;
        private IDbFactory _dbFactory;
        private CompanyDTO _company;
        private IAddressService _addressService;

        public CallEmailProcessing(ILogService log,
            IAddressService addressService,
            IDbFactory dbFactory,
            CompanyDTO company,
            ITime time)
        {
            _log = log;
            _dbFactory = dbFactory;
            _time = time;
            _company = company;
            _addressService = addressService;
        }

        public void TestCutBody()
        {
            var wasModified = false;
            var message = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "\\Files\\Emails\\email-with-amazon-footer.html");
            message = EmailHelper.CutOutBody(message, out wasModified);
            Console.WriteLine(message);
        }

        public void TestCutBody2()
        {
            var wasModified = false;
            var message = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "\\Files\\Emails\\email-with-amazon-footer2.html");
            message = EmailHelper.CutOutBody(message, out wasModified);
            Console.WriteLine(message);
        }

        public void TestBodyParse()
        {
            var body1 = "(Objet: Demande de renseignements de la part du client Amazon sarah McMillin)<br /><br />Order ID/Commande N°: 701-4152897-2930614:<br />1 of/sur Caillou Toddler Red Poly Pajamas (3T) [ASIN: B00TKJISSS] <br /><br />-------------- Begin message / Début du message -------------<br /><br />Ordered wrong size. Please send 2t.<br /><br />-------------- End message / Fin du message --------------<br /><br />For Your Information: To help arbitrate disputes and preserve trust and safety, we retain all messages buyers and sellers send through Amazon.ca.  This includes your response to the message above.<br /><br />We want you to buy with confidence anytime you purchase products on Amazon.ca. Learn more about Safe Online Shopping (http://www.amazon.ca/gp/help/customer/display.html?nodeId=13940691) and our safe buying guarantee (http://www.amazon.ca/gp/help/customer/display.html?nodeId=200342110).<br /><br />Important : Afin d'aider au règlement des litiges et de protéger la confiance et la sécurité de notre site, nous gardons une copie de tous les courriels envoyés et reçus par le biais d'Amazon.ca, y compris votre réponse au message ci-dessus.<br /><br />Il est important pour nous que vous puissiez à tout moment effectuer vos achats sur Amazon.ca en toute confiance. Pour davantage d'informations concernant la sécurité des transactions, merci de visiter la page (http://www.amazon.ca/gp/help/customer/display.html?nodeId=13940691). Pour en savoir plus sur la protection offerte par notre garantie A-Z, merci de consulter la page (http://www.amazon.ca/gp/help/customer/display.html?nodeId=200342110).<br /><br />If you believe this message is suspicious, please report it to us here: http://www.amazon.ca/gp/communication-manager/report.html?ft=InappropriateContent&msg=AYOWMP58EXOS9&d=1473100072&mp=A2EUQ1WTGCTBG2&v=1&t=038e596eeae79f50af6a239de6b6224a6de17d59<br /><br />Si vous trouvez ce message suspicieux, merci de nous le faire savoir ici : http://www.amazon.ca/gp/communication-manager/report.html?ft=InappropriateContent&msg=AYOWMP58EXOS9&d=1473100072&mp=A2EUQ1WTGCTBG2&v=1&t=038e596eeae79f50af6a239de6b6224a6de17d59<br /><br />To mark this message as no response needed, click here: http://www.amazon.ca/gp/communication-manager/no-response-needed.html?msg=AYOWMP58EXOS9&d=1473100072&mp=A2EUQ1WTGCTBG2&v=1&t=038e596eeae79f50af6a239de6b6224a6de17d59<br /><br />Pour indiquer que ce message ne requiert aucune réponse, cliquez ici : http://www.amazon.ca/gp/communication-manager/no-response-needed.html?msg=AYOWMP58EXOS9&d=1473100072&mp=A2EUQ1WTGCTBG2&v=1&t=038e596eeae79f50af6a239de6b6224a6de17d59<br /><br /><br /><br />[commMgrTok:AYOWMP58EXOS9]<br />";
            var body2 = "Yes please, I'd like to exchange it for the 14/16.<br /><br />-----Original Message-----<br />From: Premium Apparel - Amazon Marketplace [mailto:[e-mail address removed]]<br />Sent: Tuesday, September 06, 2016 9:08 AM<br />To: Haught, Laura/WDC <[e-mail address removed]><br />Subject: RE: Exchange request for order (Order: 102-5239268-9612243) [EXTERNAL]<br /><br />Order ID 102-5239268-9612243:<br />1 of Girls' Velvet Plush Cozy Robe in Variety Colors, Owls, Size 7/8 [ASIN: B01FCAWEYM]<br /><br />------------- Begin message -------------<br /><br />Unfortunately we don't have size L (10/12). Please let us know if you like to exchange to 14/16.<br /><br />--- Original message ---<br /><br />Thank you. Do you have a 12/14?<br /><br />From: Premium Apparel - Amazon Marketplace [mailto:[e-mail address removed]]<br />Sent: Tuesday, September 06, 2016 8:25 AM<br />To: Haught, Laura/WDC <[e-mail address removed]><br />Subject: Exchange request for order (Order: 102-5239268-9612243) [EXTERNAL]<br /><br /><br />Dear Mr./Mrs. Haught,<br />Thank you for your order. We are sorry the item you have received didn't fit.<br />We will be happy to exchange it free of charge for bigger size - 14/16.<br />Please let us know if you like us to send you a bigger size.<br /><br />Best Regards,<br />Premium Apparel<br />For Your Information: To help arbitrate disputes and preserve trust and safety, we retain all messages buyers and sellers send through Amazon.com for two years. This includes your response to the message above. Amazon.com uses filtering technology to protect buyers and sellers from possible fraud. Messages that fail this filtering will not be transmitted.<br /><br />We want you to buy with confidence anytime you purchase products on Amazon.com. Learn more about Safe Online Shopping (http://www.amazon.com/gp/help/customer/display.html?nodeId=551434) and our safe buying guarantee (http://www.amazon.com/gp/help/customer/display.html?nodeId=537868).<br /><br /><br /><br /><br />[Image removed by sender.]<br /><br /><br /><br /><br /><br /><br /><br />------------- End message -------------<br /><br />For Your Information: To help arbitrate disputes and preserve trust and safety, we retain all messages buyers and sellers send through Amazon.com for two years.  This includes your response to the message above.  Amazon.com uses filtering technology to protect buyers and sellers from possible fraud.  Messages that fail this filtering will not be transmitted.<br /><br />We want you to buy with confidence anytime you purchase products on Amazon.com. Learn more about Safe Online Shopping (http://www.amazon.com/gp/help/customer/display.html?nodeId=551434) and our safe buying guarantee (http://www.amazon.com/gp/help/customer/display.html?nodeId=537868).<br /><br />If you believe this message is suspicious, please report it to us here: http://www.amazon.com/gp/communication-manager/report.html?ft=InappropriateContent&msg=AF9E104T7W2Q2&d=1473167414&mp=ATVPDKIKX0DER&v=1&t=9361b955ebcbc8373fb5e6f93235a69c0f20364f<br /><br />To mark this message as no response needed, click here: http://www.amazon.com/gp/communication-manager/no-response-needed.html?msg=AF9E104T7W2Q2&d=1473167414&mp=ATVPDKIKX0DER&v=1&t=9361b955ebcbc8373fb5e6f93235a69c0f20364f<br /><br /><br /><br />[commMgrTok:AF9E104T7W2Q2]<br />";

            var subject1 = "Ship faster inquiry from Amazon customer Veera Pavan K. Kotaru (Order: 111-7366207-0755439)";

            var message = EmailHelper.ExtractShortMessageBody(body1, 200, true);
            message = EmailHelper.ExtractShortMessageBody(body2, 200, true);
            var subject = EmailHelper.ExtractShortSubject(subject1);
        }

        public void TestBodyParse2()
        {
            var body = @"Hello,
            We've been contacted by a customer regarding the order identified below.

            --------------------
            Order#: 110-3670889-2424259
            Item: Star Wars Big Boys' Vader 2-Piece Pajama Set, Black, 10 
            Reason: Tracking info

            Details: 
            Hello ,

            Please send updated tracking info for the delivery of the package. Please contact the customer regarding the details.

            Hoping for your immediate response. Thank you and have a good day!";

            var message = EmailHelper.ExtractShortMessageBody(body, 255, true);
            _log.Info(message);
        }

        public void TestBodyParse3()
        {
            var testText = @"Test<head>Remove</head>Test";
            Console.WriteLine(StringHelper.RemoveAllBetweenTags(testText, "head"));

            var body = File.ReadAllText(@"C:\Projects.Vionix\Marketplaces\MarketsSellerCentral\Amazon.InventoryUpdateManual\Files\Emails\AmazonEmail1.html");

            var noHtml = StringHelper.TrimTags(body ?? "");
            var message = StringHelper.Substring(noHtml, 70);
            var removeSignConfirmation =
                message.IndexOf("Remove signature confirmation", StringComparison.OrdinalIgnoreCase) >= 0
                || message.IndexOf("Remove signature", StringComparison.OrdinalIgnoreCase) >= 0;
            Console.WriteLine(removeSignConfirmation);
        }

        public void CheckEmailStatusNotification()
        {
            var thread = new CheckEmailStatusThread(_company.Id, 
                null,
                new List<TimeSpan>()
                {
                    new TimeSpan(7, 6, 0)
                }, 
                _time);
            thread.Start();
            Thread.Sleep(TimeSpan.FromHours(1));
        }

        public void CheckAutoResponse()
        {
            var smtpSettings = SettingsBuilder.GetSmtpSettingsFromCompany(_company);
            var emailService = new EmailService(_log, smtpSettings, _addressService);

            emailService.AutoAnswerEmails(_dbFactory,
                _time,
                _company);
        }

        public void UpdateAnsweredId()
        {
            var rule = new SetAnswerIdEmailRule(_log, _time);
            using (var db = _dbFactory.GetRWDb())
            {
                var allSentEmails = db.Emails.GetAll().Where(e => e.FolderType == (int) EmailFolders.Sent).ToList();
                var emailIds = allSentEmails.Select(e => e.Id).ToList();
                var emailToOrders = db.EmailToOrders.GetAll().Where(e => emailIds.Contains(e.EmailId)).ToList();

                foreach (var email in allSentEmails)
                {
                    rule.Process(db, new EmailReadingResult()
                    {
                        Email = new EmailDTO()
                        {
                            Id = email.Id,
                            Subject = email.Subject,
                            ReceiveDate = email.ReceiveDate,
                            Type = email.Type,
                            FolderType = email.FolderType,
                            From = email.From,
                        },
                        Status = EmailMatchingResultStatus.New,
                        Folder = "Sent",
                        MatchedIdList = emailToOrders.Where(eo => eo.EmailId == email.Id).Select(eo => eo.OrderId).ToArray()
                    });
                }
            }
        }

        public void UpdateMatchOrderId(IList<long> emailIdList)
        {
            var rule = new AddMatchIdsEmailRule(_log, _time);
            using (var db = _dbFactory.GetRWDb())
            {
                var emails = db.Emails.GetAll().Where(e => emailIdList.Contains(e.Id)).ToList();
                var emailIds = emails.Select(e => e.Id).ToList();
                var emailToOrders = db.EmailToOrders.GetAll().Where(e => emailIds.Contains(e.EmailId)).ToList();

                foreach (var email in emails)
                {
                    rule.Process(db, new EmailReadingResult()
                    {
                        Email = new EmailDTO()
                        {
                            Id = email.Id,
                            Subject = email.Subject,
                            Message = email.Message,
                            ReceiveDate = email.ReceiveDate,
                            Type = email.Type,
                            FolderType = email.FolderType,
                            From = email.From,
                        },
                        Status = EmailMatchingResultStatus.New,
                        Folder = email.FolderType == (int)EmailFolders.Inbox ? "Inbox" : "Sent",
                        MatchedIdList = emailToOrders.Where(eo => eo.EmailId == email.Id).Select(eo => eo.OrderId).ToArray()
                    });
                }
            }
        }

        public void ReadSentEmails()
        {
            var imapSettings = SettingsBuilder.GetImapSettingsFromCompany(_company);
            var smtpSettings = SettingsBuilder.GetSmtpSettingsFromCompany(_company);
            var emailService = new EmailService(_log, smtpSettings, _addressService);
            var actionService = new SystemActionService(_log, _time);

            var emailProcessing = new EmailProcessingService(_log, _dbFactory,
                emailService,
                actionService,
                _company,
                _time);

            IList<IEmailRule> sentRules = new List<IEmailRule>()
            {
                new SetSystemTypesEmailRule(_log, _time),
                new AddMatchIdsEmailRule(_log, _time),
                new SetAnswerIdEmailRule(_log, _time),
                new SetSentByEmailRule(_log, _time)
            };

            var email = new EmailReaderService(imapSettings, _log, _dbFactory, _time);
            email.ReadEmails(EmailHelper.SentFolder, DateTime.Now.AddDays(-15), null, imapSettings.AcceptingToAddresses, null);
            emailProcessing.ProcessEmails(email.EmailProcessResultList, sentRules);
        }

        public void ReadInboxEmails()
        {
            var imapSettings = SettingsBuilder.GetImapSettingsFromCompany(_company);
            var smtpSettings = SettingsBuilder.GetSmtpSettingsFromCompany(_company);
            var emailService = new EmailService(_log, smtpSettings, _addressService);
            var actionService = new SystemActionService(_log, _time);

            var emailProcessing = new EmailProcessingService(_log, _dbFactory,
                emailService,
                actionService,
                _company,
                _time);

            IList<IEmailRule> inboxRules = new List<IEmailRule>()
            {
                new SetSystemTypesEmailRule(_log, _time),
                new AddMatchIdsEmailRule(_log, _time),
                new SetAnswerIdEmailRule(_log, _time), //NOTE: also using in Inbox for processing Amazon autocommunicate emails
                //new FeedbackBlackListEmailRule(_log, _time),
                new ReturnRequestEmailRule(_log, _time, emailService, actionService, _company, true, false),
                new OrderDeliveryInquiryEmailRule(_log, _time, emailService, actionService),
                new AddressNotChangedEmailRule(_log, _time, emailService, actionService),
                //new DhlInvoiceEmailRule(_log, _time, _dbFactory),
                //new AddCommentEmailRule(_log, actionService, _time),
                new PrepareBodyEmailRule(_log, _time),
            };

            var email = new EmailReaderService(imapSettings, _log, _dbFactory, _time);
            email.ReadEmails(EmailHelper.InboxFolder, DateTime.Now.AddDays(-5), imapSettings.AcceptingToAddresses, null, null);
            emailProcessing.ProcessEmails(email.EmailProcessResultList, inboxRules);
        }

        public void SendEmails()
        {
            var settings = SettingsBuilder.GetSmtpSettingsFromAppSettings();
            var email = new EmailService(_log, settings, _addressService);
            var actionService = new SystemActionService(_log, _time);

            email.ProcessEmailActions(_dbFactory,
                _time,
                _company,
                actionService);
        }

        public void TestSendEmails()
        {
            var settings = SettingsBuilder.GetSmtpSettingsFromAppSettings();
            var email = new EmailService(_log, settings, _addressService);

            var actionServiceStub = new Mock<ISystemActionService>();
            actionServiceStub
                .Setup(l => l.GetUnprocessedByType(It.IsAny<IUnitOfWork>(), It.IsAny<SystemActionType>(), null, null))
                .Returns(new List<SystemActionDTO>()
                {
                    new SystemActionDTO()
                    {
                        InputData = SystemActionHelper.ToStr(new SendEmailInput()
                        {
                            OrderId = "106-0610855-5158626",
                            EmailType = EmailTypes.NoticeLeft,
                            Args = null
                        })
                    }
                });
            actionServiceStub
                .Setup(l => l.SetResult(It.IsAny<IUnitOfWork>(),
                    It.IsAny<long>(),
                    It.IsAny<SystemActionStatus>(),
                    It.IsAny<ISystemActionOutput>(),
                    It.IsAny<string>())).Callback((IUnitOfWork db, 
                        long actionId, 
                        SystemActionStatus status, 
                        ISystemActionOutput output, 
                        string groupId) =>
                    {
                        Console.WriteLine(SystemActionHelper.ToStr(output));
                    });

            email.ProcessEmailActions(_dbFactory,
                _time,
                _company,
                actionServiceStub.Object);
        }


        public void TestProcessSampleRemoveSignConfirmationEmail()
        {
            var smtpSettings = SettingsBuilder.GetSmtpSettingsFromAppSettings();
            var emailService = new EmailService(_log, smtpSettings, _addressService);
            var actionService = new SystemActionService(_log, _time);

            var emailProcessingService = new EmailProcessingService(
                _log,
                _dbFactory,
                emailService,
                actionService,
                _company,
                _time);

            var matchingResults = new List<EmailReadingResult>()
            {
                new EmailReadingResult()
                {
                    Email = new EmailDTO()
                    {
                        Message = "Remove signature confirmation",
                    },
                    MatchedIdList = new string[] { "701-3624993-3494603" }
                }
            };

            IList<IEmailRule> rules = new List<IEmailRule>()
            {
                new SetSystemTypesEmailRule(_log, _time),
                new AddMatchIdsEmailRule(_log, _time),
            };

            emailProcessingService.ProcessEmails(matchingResults, rules);
        }

        public void TestProcessOrderCancellationEmail(long emailId)
        {
            var smtpSettings = SettingsBuilder.GetSmtpSettingsFromAppSettings();
            var emailService = new EmailService(_log, smtpSettings, _addressService);
            var actionService = new SystemActionService(_log, _time);

            var emailProcessingService = new EmailProcessingService(
                _log,
                _dbFactory,
                emailService,
                actionService,
                _company,
                _time);

            using (var db = _dbFactory.GetRWDb())
            {
                var email = db.Emails.Get(emailId);

                var matchingResult = new EmailReadingResult()
                {
                    Email = new EmailDTO()
                    {
                        Subject = email.Subject,
                        Message = email.Message,
                    },
                    Status = EmailMatchingResultStatus.New,
                    MatchedIdList = new string[] { "4442088617765" }
                };

                IList<IEmailRule> rules = new List<IEmailRule>()
                {
                    //new SetSystemTypesEmailRule(_log, _time),
                    //new AddMatchIdsEmailRule(_log, _time),
                    new CancellationEmailRule(_log, _time, emailService, actionService, _company),
                };

                emailProcessingService.ProcessEmails(new List<EmailReadingResult>() { matchingResult }, rules);
            }
        }

        public void TestProcessSampleReturnRequestEmail()
        {
            var smtpSettings = SettingsBuilder.GetSmtpSettingsFromAppSettings();
            var emailService = new EmailService(_log, smtpSettings, _addressService);
            var actionService = new SystemActionService(_log, _time);

            var rule = new ReturnRequestEmailRule(_log, _time, emailService, actionService, _company, false, true);

            using (var db = _dbFactory.GetRWDb())
            {
                var email = db.Emails.Get(4687);

                var matchingResult = new EmailReadingResult()
                {
                    Email = new EmailDTO()
                    {
                        Subject = email.Subject,
                        Message = email.Message,
                    },
                    Status = EmailMatchingResultStatus.New,
                    MatchedIdList = new string[] { "002-1602056-5981826" }
                };
                
                rule.Process(db, matchingResult);
            }
        }


        public void RecheckAllRefundEmails()
        {
            var smtpSettings = SettingsBuilder.GetSmtpSettingsFromAppSettings();
            var emailService = new EmailService(_log, smtpSettings, _addressService);
            var actionService = new SystemActionService(_log, _time);

            var rule = new RefundEmailRule(_log, _time, emailService, actionService, _company,
                enableSendEmail: false,
                existanceCheck: true);

            using (var db = _dbFactory.GetRWDb())
            {
                var fromDate = DateTime.Now.AddDays(-35);
                var emails = db.Emails.GetAll()
                    .Where(e => e.CreateDate > fromDate
                        && e.Subject.Contains("Refund initiated for order"))
                    .ToList();
                var emailToOrderList = db.EmailToOrders.GetAll().ToList();

                foreach (var email in emails)
                {
                    var emailToOrder = emailToOrderList.FirstOrDefault(e => e.EmailId == email.Id);

                    if (emailToOrder == null)
                        continue;

                    var matchingResult = new EmailReadingResult()
                    {
                        Email = new EmailDTO()
                        {
                            Subject = email.Subject,
                            Message = email.Message,
                        },
                        Status = EmailMatchingResultStatus.New,
                        MatchedIdList = new string[] { emailToOrder.OrderId }
                    };

                    rule.Process(db, matchingResult);
                }
            }
        }

        public void RecheckAllReturnEmails()
        {
            var smtpSettings = SettingsBuilder.GetSmtpSettingsFromAppSettings();
            var emailService = new EmailService(_log, smtpSettings, _addressService);
            var actionService = new SystemActionService(_log, _time);

            var rule = new ReturnRequestEmailRule(_log, _time, emailService, actionService, _company, 
                enableSendEmail: false, 
                existanceCheck:true);

            using (var db = _dbFactory.GetRWDb())
            {
                var fromDate = DateTime.Now.AddDays(-35);
                var emails = db.Emails.GetAll()
                    .Where(e => e.CreateDate > fromDate)
                    .ToList();
                var emailToOrderList = db.EmailToOrders.GetAll().ToList();

                foreach (var email in emails)
                {
                    var emailToOrder = emailToOrderList.FirstOrDefault(e => e.EmailId == email.Id);

                    if (emailToOrder == null)
                        continue;

                    var matchingResult = new EmailReadingResult()
                    {
                        Email = new EmailDTO()
                        {
                            Subject = email.Subject,
                            Message = email.Message,
                        },
                        Status = EmailMatchingResultStatus.New,
                        MatchedIdList = new string[] { emailToOrder.OrderId }
                    };

                    rule.Process(db, matchingResult);
                }
            }
        }

        public void RecheckSetSystemTypeRule(long emailId)
        {
            var smtpSettings = SettingsBuilder.GetSmtpSettingsFromAppSettings();
            var emailService = new EmailService(_log, smtpSettings, _addressService);
            var actionService = new SystemActionService(_log, _time);

            var rule = new SetSystemTypesEmailRule(_log, _time);

            using (var db = _dbFactory.GetRWDb())
            {
                var fromDate = DateTime.Now.AddDays(-35);
                var emails = db.Emails.GetAll()
                    .Where(e => e.Id == emailId)
                    .ToList();
                var emailToOrderList = db.EmailToOrders.GetAll().ToList();

                foreach (var email in emails)
                {
                    var emailToOrder = emailToOrderList.FirstOrDefault(e => e.EmailId == email.Id);

                    if (emailToOrder == null)
                        continue;

                    var matchingResult = new EmailReadingResult()
                    {
                        Email = new EmailDTO()
                        {
                            From = email.From,
                            Subject = email.Subject,
                            Message = email.Message,
                        },
                        Status = EmailMatchingResultStatus.New,
                        MatchedIdList = new string[] { emailToOrder.OrderId }
                    };

                    rule.Process(db, matchingResult);
                }
            }
        }
    }
}
