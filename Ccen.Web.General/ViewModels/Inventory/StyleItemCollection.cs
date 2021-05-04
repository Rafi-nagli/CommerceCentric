using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Amazon.Web.ViewModels.Inventory
{
    public class StyleItemCollection
    {
        public string Title { get; set; }
        public StyleItemDisplayMode DisplayMode { get; set; }
        public IList<StyleItemViewModel> Items { get; set; }

        public StyleItemCollection()
        {
            DisplayMode = StyleItemDisplayMode.Standard;
            Items = new List<StyleItemViewModel>();            
        }

        public StyleItemCollection(StyleItemDisplayMode displayMode)
        {
            DisplayMode = displayMode;
            Items = new List<StyleItemViewModel>();
        }
    }
}