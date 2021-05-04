using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Core;
using Amazon.DTO;

namespace Amazon.Web.ViewModels.Inventory.Boxes
{
    public class BoxSizeWizardViewModel
    {
        public long Id { get; set; }

        public string Size { get; set; }

        public string Color { get; set; }

        public int? SizeId { get; set; }
        public int? SizeGroupId { get; set; }
        public string SizeGroupName { get; set; }


        //Input
        public int? Quantity { get; set; }
        public int? Breakdown { get; set; }
        public bool UseBoxQuantity { get; set; }


        public override string ToString()
        {
            return "Id=" + Id
                   + ", Size=" + Size
                   + ", Color=" + Color
                   + ", Quantity=" + Quantity
                   + ", Breakdown=" + Breakdown
                   + ", UseBoxQuantity=" + UseBoxQuantity;
        }

        public BoxSizeWizardViewModel()
        {

        }

        public BoxSizeWizardViewModel(StyleItemDTO styleItem)
        {
            Id = styleItem.StyleItemId;
            Size = styleItem.Size;
            Color = styleItem.Color;

            SizeId = styleItem.SizeId;
            SizeGroupId = styleItem.SizeGroupId;
            SizeGroupName = styleItem.SizeGroupName;

            UseBoxQuantity = !styleItem.Quantity.HasValue;
        }
    }
}