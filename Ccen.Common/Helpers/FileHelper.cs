using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using ZipFile = ICSharpCode.SharpZipLib.Zip.ZipFile;
using Zip = System.IO.Compression.ZipFile;
using System.IO.Compression;

namespace Amazon.Common.Helpers
{
    public class FileHelper
    {
        public static void MoveFolderFiles(string sourceFolder, string destPath)
        {
            var folderName = Path.GetFileName(sourceFolder);
            var destFolder = Path.Combine(destPath, folderName);
            if (!Directory.Exists(destFolder))
            {
                Directory.CreateDirectory(destFolder);
            }
            var filesToMove = Directory.GetFiles(sourceFolder);
            foreach (var file in filesToMove)
            {
                var filename = Path.GetFileName(file);
                var destFile = Path.Combine(destFolder, filename);
                if (File.Exists(destFile))
                {
                    File.Delete(destFile);
                }
                File.Move(file, destFile);
            }
        }
                            

        public static void SaveToFile(string filepath, string date)
        {
            using (var stream = File.Create(filepath))
            {
                using (var sw = new StreamWriter(stream))
                {
                    sw.Write(date);
                }
            }
        }

        public static string GetNameFromUrl(string url)
        {
            var uri = new Uri(url);
            var filename = Path.GetFileName(uri.AbsolutePath);
            filename = HttpUtility.UrlDecode(filename);
            return filename;
        }

        public class ZipFileInfo
        {
            public string SourceFilepath { get; set; }
            public string InArchiveFilename { get; set; }
        }

        public static void ZipTo(string archiveFilename, ZipFileInfo[] toArchiveFiles)
        {
            var zip = Zip.Open(archiveFilename, ZipArchiveMode.Create);
            foreach (var file in toArchiveFiles)
            {
                var filename = file.InArchiveFilename;
                if (String.IsNullOrEmpty(filename))
                    filename = Path.GetFileName(file.SourceFilepath);
                zip.CreateEntryFromFile(file.SourceFilepath, filename, CompressionLevel.Optimal);
            }
            // Dispose of the object when we are done
            zip.Dispose();
        }

        public static void UnzipTo(string filename, Stream toStream)
        {
            ZipFile zf = null;
            try
            {
                FileStream fs = File.OpenRead(filename);
                zf = new ZipFile(fs);

                ZipEntry zipEntry = zf[0];

                String entryFileName = zipEntry.Name;

                byte[] buffer = new byte[4096];     // 4K is optimum
                Stream zipStream = zf.GetInputStream(zipEntry);

                StreamUtils.Copy(zipStream, toStream, buffer);
            }
            finally
            {
                if (zf != null)
                {
                    zf.IsStreamOwner = true; // Makes close also shut the underlying stream
                    zf.Close(); // Ensure we release resources
                }
            }
        }

        public static void UnzipToFolder(string filename, string destFolder)
        {
            FastZip fastZip = new FastZip();
            string fileFilter = null;

            // Will always overwrite if target filenames already exist
            fastZip.ExtractZip(filename, destFolder, fileFilter);
        }

        public static string PrepareFileName(string filename)
        {
            if (String.IsNullOrEmpty(filename))
                return filename;

            return filename.Replace("?", "_")
                .Replace("=", "_")
                .Replace(".", "_")
                .Replace("$", "_")
                .Replace("#", "_")
                .Replace("+", "_")
                .Replace(" ", "_")
                .Replace(":", "_")
                .Replace("&", "_")
                .Replace("%", "_")
                .Replace("(", "_")
                .Replace(")", "_");
        }

        public static string ToDirectoryNameWithBackslash(string directoryName)
        {
            if (String.IsNullOrEmpty(directoryName))
                return directoryName;

            if (directoryName.EndsWith("/") || directoryName.EndsWith("\\"))
                return directoryName;
            return directoryName + "/";
        }

        public static string GetNotExistFileName(string dictionary, string fileName)
        {
            int index = 1;
            string newFilename = fileName;
            while (File.Exists(Path.Combine(dictionary, newFilename)))
            {
                newFilename = Path.GetFileNameWithoutExtension(fileName) + "_" + index + Path.GetExtension(fileName);
                index++;
            }
            return newFilename;
        }

        public static string GetMimeTypeByExt(string ext)
        {
            string mime = "";
            ext = (ext ?? "").ToLower();

            switch (ext)
            {
                //TODO: excel

                case ".xls":
                    mime = "application/vnd.ms-excel";
                    break;
                case ".xlsx":
                    mime = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    break;

                case ".csv":
                    mime = "text/csv";
                    break;

                case ".pdf":
                    mime = "application/pdf";
                    break;

                case ".bmp":
                    mime = "image/bmp";
                    break;

                case ".png":
                    mime = "image/png";
                    break;

                case ".css":
                    mime = "text/css";
                    break;

                case ".gif":
                    mime = "image/gif";
                    break;

                case ".ico":
                    mime = "image/x-icon";
                    break;

                case ".htm":
                case ".html":
                    mime = "text/html";
                    break;

                case ".xml":
                    mime = "text/xml";
                    break;

                case ".jpe":
                case ".jpeg":
                case ".jpg":
                    mime = "image/jpeg";
                    break;

                case ".zpl":
                    mime = "application/octet-stream";
                    break;

                case ".js":
                    mime = "application/x-javascript";
                    break;
            }

            return mime;
        }

        public static void WriteToFile(byte[] data, string filename)
        {
            var filepath = filename;
            using (var fileStream = File.Open(filepath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                fileStream.Write(data, 0, data.Length);
                fileStream.Flush();
            }
        }

        public static MemoryStream DownloadRemoteFileAsStream(string uri)
        {
            var memoryStream = new MemoryStream();

            var request = (HttpWebRequest)WebRequest.Create(uri);
            using (var response = (HttpWebResponse) request.GetResponse())
            {
                if ((response.StatusCode == HttpStatusCode.OK ||
                     response.StatusCode == HttpStatusCode.Moved ||
                     response.StatusCode == HttpStatusCode.Redirect))
                {

                    // if the remote file was found, download oit
                    using (var inputStream = response.GetResponseStream())
                    {
                        if (inputStream != null)
                        {
                            var buffer = new byte[4096];
                            int bytesRead;
                            do
                            {
                                bytesRead = inputStream.Read(buffer, 0, buffer.Length);
                                memoryStream.Write(buffer, 0, bytesRead);
                            } while (bytesRead != 0);
                        }
                    }

                    return memoryStream;
                }
                else
                {
                    return null;
                }
            }
        }

        public static string XlsFormat = "xls";
        public static string XlsxFormat = "xlsx";
        public static string CsvFormat = "csv";

        public static string GetFormat(string fileext)
        {
            var format = (fileext ?? "").Replace(".", "").ToLower();

            return format;
        }

        public static DateTime? GetFileCreateDate(string filepath)
        {
            try
            {
                return File.GetCreationTime(filepath);
            }
            catch
            {
                return null;
            }
        }
    }
}
