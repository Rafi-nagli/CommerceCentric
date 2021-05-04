using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ccen.Web.ViewModels.CustomReports
{
    public class DisplayInGridAttribute : Attribute
    { 
        public int Width { get; set; }
        public DisplayTypeEnum DisplayType { get; set; }
        //public string DataType { get; set; }
        public string Title { get; set; }
        public bool Visible { get; set; }        
        public int Order { get; set; }

        public DisplayInGridAttribute(string title, DisplayTypeEnum displayType, int width, int order)
        {
            Width = width;
            DisplayType = displayType;
            Title = title;            
            Visible = width > 0;
            Order = order;
        }

        

        public DisplayInGridAttribute(DisplayTypeEnum displayType, int order, int width=0)
        {
            DisplayType = displayType;
            Order = order;
            Visible = true;
            Width = width;
        }

        public DisplayInGridAttribute()
        {

        }
    }

    public enum DisplayTypeEnum
    {
        DateTime,
        General,
        Image,
        LinkButton,
        Button,
        Link
    }
}