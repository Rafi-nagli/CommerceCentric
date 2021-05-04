using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Amazon.Web.ViewModels.Graph
{
    public class FeatureGraphPageViewModel
    {
        public string HtmlPrefix { get; set; }
        public int FeatureId { get; set; }
        public string SeriesName { get; set; }
        public bool EnableSubFeature { get; set; }
    }
}