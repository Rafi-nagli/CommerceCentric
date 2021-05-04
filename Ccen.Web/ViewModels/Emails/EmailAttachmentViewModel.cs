using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.DTO.Emails;
using Amazon.Utils;
using Amazon.Web.Models;

namespace Amazon.Web.ViewModels.Emails
{
    public class EmailAttachmentViewModel
    {
        public long Id { get; set; }
        public string SourceFileName { get; set; }
        public string ServerFileName { get; set; }
        public string Title { get; set; }

        public string ViewUrl { get; set; }
        public EmailAttachmentViewModel()
        {
            
        }

        public EmailAttachmentViewModel(EmailAttachmentDTO attachment)
        {
            Id = attachment.Id;
            ServerFileName = attachment.FileName;
            Title = attachment.Title;
            ViewUrl = UrlHelper.GetViewAttachmentUrl(Id);
        }
        
        public static string GetAttachmentFilepath(IUnitOfWork db, long id)
        {
            var attachment = db.EmailAttachments.GetAllAsDto().FirstOrDefault(e => e.Id == id);
            if (attachment != null)
                return Path.Combine(Models.UrlHelper.GetEmailAttachemntPath(), attachment.RelativePath.Trim("~/\\".ToCharArray()));
            return null;
        }

        public static EmailAttachmentViewModel GetStyleImageAsAttachment(IUnitOfWork db,
            ILogService log,
            ITime time,
            string styleString)
        {
            var style = db.Styles.GetActiveByStyleIdAsDto(styleString);
            if (style != null && !String.IsNullOrEmpty(style.Image))
            {
                try
                {
                    var path = style.Image;
                    var fileExt = Path.GetExtension(Models.UrlHelper.RemoveUrlParams(path));
                    var sourceFileName = style.StyleID + "_" + time.GetAppNowTime().ToString("yyyyMMddHHmmsss") + fileExt;
                    
                    var dir = Models.UrlHelper.GetUploadEmailAttachmentPath();
                    var fileName = FileHelper.GetNotExistFileName(dir, sourceFileName);
                    var destinationPath = Path.Combine(dir, fileName);

                    ImageHelper.DownloadRemoteImageFile(style.Image, destinationPath, null);

                    return new EmailAttachmentViewModel()
                    {
                        ViewUrl = style.Image,
                        ServerFileName = fileName,
                    };
                }
                catch (Exception ex)
                {
                    log.Error("GetStyleImageAsAttachment, styleString=" + styleString, ex);
                }
            }
            return null;
        }
    }
}