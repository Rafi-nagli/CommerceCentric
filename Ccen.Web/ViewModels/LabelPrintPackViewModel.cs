using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Amazon.Core.Contracts.Db;
using Amazon.Core.Entities;
using Amazon.Web.Models;

namespace Amazon.Web.ViewModels
{
    public class LabelPrintPackViewModel
    {
        public long Id { get; set; }
        public string FileName { get; set; }
        public DateTime CreateDate { get; set; }
        public int? LabelsNumber { get; set; }
        public long? BatchId { get; set; }
        public string SinglePackPersonName { get; set; }
        public bool IsReturn { get; set; }

        public string NumberOrPerson
        {
            get
            {
                var batchPrefix = (BatchId.HasValue && BatchId > 0) ? BatchId.ToString() + " - " : "";

                return batchPrefix + 
                    (string.IsNullOrEmpty(SinglePackPersonName)
                    ? LabelsNumber != null ? LabelsNumber.ToString() : ""
                    : (IsReturn ? "Return " : "") + SinglePackPersonName);
            }
        }

        public string FormattedFileName
        {
            get { return Path.GetFileName(UrlHelper.GetLocalPath(FileName)); }
        }

        public string Url
        {
            get { return UrlHelper.GetLabelPath(FileName); }
        }

        public string FormattedCreateDate
        {
            get { return CreateDate.ToString("MM-dd-yyyy HH:mm:ss"); }
        }

        public LabelPrintPackViewModel()
        {
            
        }

        public LabelPrintPackViewModel(LabelPrintPack pack)
        {
            Id = pack.Id;
            FileName = pack.FileName;
            CreateDate = pack.CreateDate;
            SinglePackPersonName = pack.PersonName;
            LabelsNumber = pack.NumberOfLabels;
            BatchId = pack.BatchId;
            IsReturn = pack.IsReturn;
        }

        public static IList<LabelPrintPackViewModel> GetAll(ILabelPrintPackRepository r)
        {
            return r.GetAll().ToList().Select(l => new LabelPrintPackViewModel(l)).ToList();
        }
    }
}