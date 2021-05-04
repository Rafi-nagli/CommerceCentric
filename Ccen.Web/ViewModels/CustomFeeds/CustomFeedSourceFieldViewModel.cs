using Amazon.Common.ExcelExport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ccen.Web.ViewModels.CustomFeeds
{
    public class CustomFeedSourceFieldViewModel
    {
        public string FieldName { get; set; }

        public static List<CustomFeedSourceFieldViewModel> Build(IList<string> fields)
        {
            return fields.Select(f => new CustomFeedSourceFieldViewModel()
            {
                FieldName = f
            }).ToList();
        }

        public static List<CustomFeedSourceFieldViewModel> BuildFromFile(string filename)
        {
            var columnNames = ExcelHelper.GetHeaders(filename);
            return Build(columnNames);
        }
    }
}