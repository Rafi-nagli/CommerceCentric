using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Inventory
{
    public class PhotoshootPickListEntryDTO
    {
        public long Id { get; set; }

        public long PhotoshootPickListId { get; set; }

        public long StyleId { get; set; }
        public string StyleString { get; set; }

        public long StyleItemId { get; set; }

        public int TakenQuantity { get; set; }
        public int ReturnedQuantity { get; set; }

        public int Status { get; set; }
        public DateTime? StatusDate { get; set; }
        public long? StatusBy { get; set; }

        public DateTime CreateDate { get; set; }
        public long? CreatedBy { get; set; }
    }
}
