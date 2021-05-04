using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Amazon.Common.Helpers;
using Amazon.Core.Contracts;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Models;
using Amazon.Core.Models.Stamps;
using iTextSharp.awt.geom;
using iTextSharp.text;
using iTextSharp.text.pdf;
using static Amazon.Common.Helpers.FileHelper;
using Image = iTextSharp.text.Image;

namespace Amazon.Model.Implementation.Pdf
{
    public class PngZipArchiveMaker : IFileMaker
    {
        private ILogService _log;

        public PngZipArchiveMaker(ILogService log)
        {
            _log = log;
        }

        public string CreateFileWithLabels(IList<PrintLabelInfo> labels, 
            IList<string> scanFormImages, 
            BatchInfoToPrint batchInfo,
            string outputDirectory, 
            string name = null)
        {
            var fileName = String.Format("label_{0}.zip", 
                StringHelper.JoinTwo("_", 
                    (batchInfo != null && batchInfo.BatchId > 0) ? batchInfo.BatchId.ToString() : null, 
                    DateHelper.GetAppNowTime().ToString("MM_dd_yyyy_HH_mm_ss")));

            var localPath = outputDirectory + "\\LabelPdf\\" + fileName;
            Directory.CreateDirectory(Path.GetDirectoryName(localPath));

            var labelFiles = labels.Select(f => new ZipFileInfo()
            {
                SourceFilepath = f.Image,
                InArchiveFilename = f.OrderId + Path.GetExtension(f.Image)
            }).ToArray();
            FileHelper.ZipTo(localPath, labelFiles);
            
            return "~\\LabelPdf\\" + fileName;
        }
    }
}