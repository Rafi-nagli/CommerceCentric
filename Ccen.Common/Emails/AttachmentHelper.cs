using Amazon.Common.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;

namespace Amazon.Model.Implementation.Emails
{
    public class AttachmentHelper
    {
        public static string SaveMailAttachment(AlternateView view, string attachDir, string dateDir)
        {
            StreamReader r = new StreamReader(view.ContentStream, Encoding.Default);
            var data = r.ReadToEnd();
            var allBytes = new byte[0];

            //if (view.TransferEncoding == System.Net.Mime.TransferEncoding.Base64)
            //{
            //    allBytes = StringHelper.FromBase64(data);
            //}
            //else
            //{
                view.ContentStream.Seek(0, SeekOrigin.Begin);
                allBytes = new byte[view.ContentStream.Length];
                view.ContentStream.Read(allBytes, 0, (int)view.ContentStream.Length);
            //}            

            var destinationFile = GetDestinationFileName(attachDir, dateDir, view.ContentId + GetExtFromMediaType(view.ContentType?.MediaType));

            var writer = new BinaryWriter(new FileStream(destinationFile, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None));
            writer.Write(allBytes);
            writer.Close();

            return destinationFile;
        }

        private static string GetExtFromMediaType(string mediaType)
        {
            if (mediaType == "image/jpeg")
                return ".jpg";
            return "";
        }

        public static string SaveMailAttachment(Attachment attachment, string attachDir, string dateDir)
        {
            var allBytes = new byte[attachment.ContentStream.Length];
            attachment.ContentStream.Read(allBytes, 0, (int)attachment.ContentStream.Length);

            var destinationFile = GetDestinationFileName(attachDir, dateDir, attachment.Name);

            var writer = new BinaryWriter(new FileStream(destinationFile, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None));
            writer.Write(allBytes);
            writer.Close();

            return destinationFile;
        }

        private static string GetDestinationFileName(string attachDir, string dateDir, string name)
        {
            var destinationDir = Path.Combine(attachDir, dateDir);
            if (!Directory.Exists(destinationDir))
            {
                Directory.CreateDirectory(destinationDir);
            }

            var destinationFile = Path.Combine(destinationDir, name);
            var fileExtention = Path.GetExtension(destinationFile);
            var fileName = Path.GetFileNameWithoutExtension(destinationFile);
            int i = 0;
            while (File.Exists(destinationFile))
            {
                i++;
                destinationFile = Path.Combine(destinationDir,
                                               string.Format("{0}_({1}){2}", fileName, i, fileExtention));
            }

            return destinationFile;
        }
    }
}
