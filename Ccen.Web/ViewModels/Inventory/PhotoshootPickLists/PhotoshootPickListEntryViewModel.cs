using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.DTO.Inventory;

namespace Amazon.Web.ViewModels.Inventory.FBAPickLists
{
    public class PhotoshootPickListEntryViewModel
    {
        public long Id { get; set; }

        //Input
        public long StyleId { get; set; }
        public string StyleString { get; set; }
        public long StyleItemId { get; set; }
        public int TakenQuantity { get; set; }
        public int ReturnedQuantity { get; set; }

        public int Status { get; set; }
        public DateTime? StatusDate { get; set; }
        public long? StatusBy { get; set; }

        public PhotoshootPickListEntryViewModel()
        {
            
        }

        public PhotoshootPickListEntryViewModel(PhotoshootPickListEntryDTO item)
        {
            Id = item.Id;

            StyleId = item.StyleId;
            StyleString = item.StyleString;
            StyleItemId = item.StyleItemId;
            TakenQuantity = item.TakenQuantity;
            ReturnedQuantity = item.ReturnedQuantity;
            Status = item.Status;
            StatusDate = item.StatusDate;
            StatusBy = item.StatusBy;
        }
    }
}