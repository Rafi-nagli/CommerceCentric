using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.DTO.Inventory;

namespace Amazon.DTO
{
    //TODO: Move all additional fields
    public class ItemExDTO : ItemDTO
    {
        public string Description { get; set; }

        public bool IsChild { get; set; }
        public bool IsParent { get; set; }
        public string ParentSKU { get; set; }

        public bool IsCheckedSpecialSize { get; set; }
        public string SpecialSize { get; set; }

        public decimal? WinnerPrice { get; set; }
        public DateTime? WinnerPriceLastChangeDate { get; set; }
        public decimal? WinnerPriceLastChangeValue { get; set; }

        public decimal? WinnerSalePrice { get; set; }
        public DateTime? WinnerSalePriceLastChangeDate { get; set; }
        public decimal? WinnerSalePriceLastChangeValue { get; set; }
    }
}
