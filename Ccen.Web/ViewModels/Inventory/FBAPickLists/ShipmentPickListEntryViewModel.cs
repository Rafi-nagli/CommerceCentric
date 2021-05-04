using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.DTO.Inventory;

namespace Amazon.Web.ViewModels.Inventory.FBAPickLists
{
    public class ShipmentPickListEntryViewModel
    {
        public long Id { get; set; }

        //Input
        public long StyleId { get; set; }
        public string StyleString { get; set; }
        public long StyleItemId { get; set; }
        public int Quantity { get; set; }
        public long ListingId { get; set; }

        public ShipmentPickListEntryViewModel()
        {
            
        }

        public ShipmentPickListEntryViewModel(FBAPickListEntryDTO item)
        {
            Id = item.Id;

            StyleId = item.StyleId;
            StyleString = item.StyleString;
            StyleItemId = item.StyleItemId;
            Quantity = item.Quantity;
            ListingId = item.ListingId;
        }
    }
}