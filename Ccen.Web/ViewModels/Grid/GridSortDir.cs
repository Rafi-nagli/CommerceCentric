using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Helpers;

namespace Ccen.Web.ViewModels.Grid
{
    public class GridSortDir
    {
        public GridSortDir()
        {
            
        }

        public string field { get; set; }
        public string dir { get; set; }
    }
}