using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Core.Models.Calls;

namespace Amazon.Web.ViewModels.Products.Edits
{
    public class StyleVariationListViewModel
    {
        public string StyleString { get; set; }
        public string Name { get; set; }

        public IList<MessageString> WalmartLookupMessages { get; set; }

        public string WalmartLookupSummary
        {
            get
            {
                if (WalmartLookupMessages != null)
                    return String.Join("<br/>", WalmartLookupMessages.Select(m => m.Message));
                return "";
            }
        }

        public IList<ItemVariationEditViewModel> Variations { get; set; }
    }
}