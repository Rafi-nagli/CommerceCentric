using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.DTO;
using Amazon.DTO.Emails;
using Amazon.Model.Implementation.Emails;
using S22.Imap;

namespace Amazon.Model.Implementation
{
    public class EmailReaderService
    {
        private IEmailImapSettings _settings;
        private ILogService _log;
        private ITime _time;
        private IDbFactory _dbFactory;

        private static IList<Exception> _threadExceptions = new List<Exception>();

        private IList<EmailDTO> newUidList = new List<EmailDTO>();
        private IList<EmailDTO> existingUidList = new List<EmailDTO>();

        private List<EmailReadingResult> _emailProcessResultList = new List<EmailReadingResult>();
        public IList<EmailReadingResult> EmailProcessResultList
        {
            get { return _emailProcessResultList.AsReadOnly(); }
        }


        public EmailReaderService(IEmailImapSettings settings, 
            ILogService log,
            IDbFactory dbFactory,
            ITime time)
        {
            _settings = settings;
            _log = log;
            _dbFactory = dbFactory;
            _time = time;
        }

        public void ReadEmails(string folder, 
            DateTime? fromDate,
            IList<string> acceptingToAddresses,
            IList<string> acceptingFromAddresses,
            CancellationToken? cancel)
        {
            _emailProcessResultList = new List<EmailReadingResult>();

            using (var imap = new ImapClient(_settings.ImapHost,
                _settings.ImapPort,
                _settings.ImapUsername,
                _settings.ImapPassword,
                AuthMethod.Login,
                ssl: true))
            {
                //var uids = new List<uint>(ImapClient.Search(SearchCondition.SentSince(sentSince).Or(SearchCondition.GreaterThan(lastUid)), mailbox));

                //var uids = new List<uint>(ImapClient.Search(SearchCondition.From("Noemi.Rocha@lifetimebrands.com"), _mailbox));
                //var uids = new List<uint> {lastUid};// 14559 };
                //var uids = new List<uint>(ImapClient.Search(SearchCondition.SentSince(new DateTime(2013, 12, 3)).Or(SearchCondition.GreaterThan(105748)), mailbox));//.SentSince(sentSince.AddHours(-12)), mailbox));


                var sentSince = fromDate ?? _time.GetUtcTime().AddDays(-2);
                using (var db = _dbFactory.GetRWDb())
                {
                    DateTime? maxEmailDate = db.Emails.GetAll().Max(e => e.ReceiveDate);
                    if (maxEmailDate.HasValue)
                        sentSince = maxEmailDate.Value.AddHours(-5);
                }
                    
                uint lastUid = 0;

                //_log.Info(String.Join("; ", imap.ListMailboxes()));
                //var uids = new List<uint>(imap.Search(SearchCondition.SentSince(sentSince).Or(SearchCondition.GreaterThan(lastUid)), "INBOX"));
                var uids = new List<uint>();
                if (_settings.IsDebug)
                {
                    uids = new List<uint>() { 30456 };
                    //  imap.Search(SearchCondition.Subject("kimlimu_cecku3g5 sent a message about Peppa Pig Little Girls"), folder).ToList();}
                }
                else
                {
                    SearchCondition toCriteria = null;
                    if (acceptingToAddresses != null && acceptingToAddresses.Any())
                    {
                        foreach (var to in acceptingToAddresses)
                            toCriteria = toCriteria == null ? SearchCondition.To(to) : toCriteria.Or(SearchCondition.To(to));
                    }

                    SearchCondition fromCriteria = null;
                    if (acceptingFromAddresses != null && acceptingFromAddresses.Any())
                    {
                        foreach (var from in acceptingFromAddresses)
                            fromCriteria = fromCriteria == null ? SearchCondition.From(from) : fromCriteria.Or(SearchCondition.From(from));
                    }

                    var searchCriteria = SearchCondition.SentSince(sentSince);
                    if (toCriteria != null)
                        searchCriteria = searchCriteria.And(toCriteria);
                    if (fromCriteria != null)
                        searchCriteria = searchCriteria.And(fromCriteria);
                    uids = new List<uint>(imap.Search(searchCriteria, folder));
                }


                foreach (var uid in uids)
                {
                    if (cancel.HasValue && cancel.Value.IsCancellationRequested)
                    {
                        _log.Info("Cancellation Requested!");
                        cancel.Value.ThrowIfCancellationRequested();
                    }


                    _log.Info("Begin check uid: " + uid);
                    var emailProcessingThread = new Thread(() => GetEmail(_dbFactory, imap, _time.GetUtcTime(), uid, folder));
                    emailProcessingThread.Priority = ThreadPriority.Highest;
                    emailProcessingThread.Start();

                    if (_settings.IsDebug)
                    {
                        emailProcessingThread.Join();
                    }
                    else
                    {
                        if (!emailProcessingThread.Join(_settings.ProcessMessageThreadTimeout))
                        {
                            emailProcessingThread.Abort();
                            throw new Exception("Timeout exceeded while processing email. Uid:" + uid);
                        }
                    }
                }

                Console.WriteLine(uids.Count);
            }
        }

        private void GetEmail(IDbFactory dbFactory, ImapClient imap, DateTime scanDate, uint uid, string folder)
        {
            _threadExceptions.Clear();

            using (var db = dbFactory.GetRWDb())
            {
                //TODO: checking by UID only if UIDValidity the same after select folder
                var folderType = EmailHelper.GetFolderType(folder);
                var existEmail = db.Emails.GetByUid(uid, folderType);

                if (existEmail == null)
                {
                    _log.Info("Start email processing, uid: " + uid + ", folder=" + folder);
                    try
                    {
                        EmailReadingResult readingResult = null;

                        _log.Info("Start GetMessageHeaders()");
                        var messageHeaders = imap.GetMessage(uid, FetchOptions.HeadersOnly, false, folder);
                        _log.Info("Eng GetMessageHeaders()");

                        //TODO: Now not working (no logic to read labels from headers)
                        var labels = EmailParserHelper.GetMessageLabels(messageHeaders.Headers);
                        _log.Info("Labels: " + labels);

                        var messageID = EmailParserHelper.GetMessageID(messageHeaders.Headers);
                        var existPOEmail = db.Emails.GetByMessageID(messageID);
                        if (existPOEmail != null)
                        {
                            db.Emails.UpdateUID(existPOEmail.Id, uid);
                            _log.Info("UID updated:" + uid + "for exist email, subject: " + existPOEmail.Subject);

                            readingResult = new EmailReadingResult()
                            {
                                Email = existPOEmail,
                                Headers = messageHeaders.Headers,
                                Folder = folder,
                                Status = EmailMatchingResultStatus.Existing
                            };
                        }
                        else
                        {
                            _log.Info("Start GetMessage()");
                            var message = imap.GetMessage(uid, FetchOptions.Normal, false, folder);
                            _log.Info("Eng GetMessage(). Subject: " + message.Subject + Environment.NewLine);

                            readingResult = SaveEmail(db, message, uid, folder, scanDate);
                        }

                        _emailProcessResultList.Add(readingResult);

                        switch (readingResult.Status)
                        {
                            case EmailMatchingResultStatus.New:
                                newUidList.Add(new EmailDTO()
                                {
                                    Id = readingResult.Email.Id,
                                    UID = readingResult.Email.UID
                                });
                                break;
                            case EmailMatchingResultStatus.Existing:
                                existingUidList.Add(new EmailDTO()
                                {
                                    Id = readingResult.Email.Id,
                                    UID = readingResult.Email.UID
                                });
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        _log.Fatal(string.Format("Not processed Uid: {0}", uid), ex);
                        _threadExceptions.Add(new Exception(string.Format("Not processed Uid: {0}", uid), ex));
                    }

                    _log.Info("End email processing. Uid: " + uid);
                }
                else
                {
                    existingUidList.Add(new EmailDTO()
                    {
                        Id = existEmail.Id,
                        UID = uid
                    });
                }
            }
        }

        private EmailReadingResult SaveEmail(IUnitOfWork db,
            MailMessage mailMessage,
            uint uid,
            string folder,
            DateTime scanDate)
        {
            var email = EmailParserHelper.ParseEmail(mailMessage, uid, folder, scanDate);

            _log.Info("Start inserting Email. Subject: " + email.Subject);
            db.Emails.Insert(email);
            _log.Info("End inserting Email");
            
            var attachments = ProcessAttachments(db, mailMessage, email.Id);
            _log.Info("End inserting Attachments");

            email.Attachments = attachments;

            return new EmailReadingResult()
            {
                Email = email,
                Headers = mailMessage.Headers,
                Folder = folder,
                Status = EmailMatchingResultStatus.New
            };
            
        }

        private IList<EmailAttachmentDTO> ProcessAttachments(IUnitOfWork db, MailMessage email, long emailId)
        {
            var results = new List<EmailAttachmentDTO>();

            var attachFolder = _settings.AttachmentFolderPath;
            var date = DateHelper.GetAppNowTime();
            var dateDir = string.Format("{0}_{1}_{2}", date.Month, date.Day, date.Year);

            var attachments = email.Attachments.Select(a => a);
            foreach (var attachment in attachments)
            {
                try
                {
#if DEBUG 
                    attachFolder = "D:\\AmazonOutput\\EmailAttachments\\";
#endif
                    var filePath = AttachmentHelper.SaveMailAttachment(attachment, attachFolder, dateDir);
                    var filename = Path.GetFileName(filePath);
                    var attach = new EmailAttachmentDTO()
                    {
                        EmailId = emailId,
                        FileName = filename,
                        Title = attachment.Name,
                        PhysicalPath = filePath,
                        RelativePath = "~/" + dateDir + "/" + filename,
                        CreateDate = date
                    };
                    db.EmailAttachments.Insert(attach);
                    results.Add(attach);
                }
                catch (Exception ex)
                {
                    _log.Error("Failed to save attachment " + attachment.Name + " for reason: " + ex);
                }
            }

            var alternateViews = email.AlternateViews.Where(a => a.ContentType?.MediaType == "image/jpeg").Select(a => a);
            foreach (var view in alternateViews)
            {
                try
                {
                    var filePath = AttachmentHelper.SaveMailAttachment(view, attachFolder, dateDir);
                    var filename = Path.GetFileName(filePath);
                    var attach = new EmailAttachmentDTO()
                    {
                        EmailId = emailId,
                        FileName = filename,
                        Title = view.ContentId,
                        PhysicalPath = filePath,
                        RelativePath = "~/" + dateDir + "/" + filename,
                        CreateDate = date
                    };
                    db.EmailAttachments.Insert(attach);
                    results.Add(attach);
                }
                catch (Exception ex)
                {
                    _log.Error("Failed to save attachment " + view.ContentId + " for reason: " + ex);
                }
            }

            return results;
        }
    }
}
