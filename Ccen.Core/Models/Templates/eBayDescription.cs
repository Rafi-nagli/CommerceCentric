using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.DTO.Inventory;

namespace Amazon.Core.Models.Templates
{
    public class eBayDescriptionTemplate
    {
        public string Title { get; set; }
        public string ImageUrl { get; set; }

        public IList<string> Images { get; set; }
        public string Description { get; set; }
        public string BrandName { get; set; }
        public string ItemStyle { get; set; }
        public string Color { get; set; }
        public string Material { get; set; }
        public string Seasons { get; set; }

        public IList<FeatureValueDTO> Features { get; set; }
    }
}
