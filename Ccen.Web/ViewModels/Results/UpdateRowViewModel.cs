using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Kendo.Mvc.UI;

namespace Amazon.Web.ViewModels.Inventory
{
    public class UpdateRowViewModel
    {
        public object Row { get; set; }
        public string GridName { get; set; }
        public string[] UpdateFields { get; set; }
        public bool ForseGridRefresh { get; set; }

        public string Url { get; set; } //OpenUrl

        public UpdateRowViewModel(object row, string gridName, string[] updateFields, bool forseGridRefresh, string openUrl = "")
        {
            Row = row;
            GridName = gridName;
            UpdateFields = updateFields;
            ForseGridRefresh = forseGridRefresh;
            Url = openUrl;
        }
    }
}