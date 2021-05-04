using Ccen.DTO.Inventory;
using System;

namespace Amazon.DTO.Inventory
{
    public class FBAPickListDTO
    {
        public long Id { get; set; }

        public string FBAPickListType { get; set; }
        public ShipmentStatuses Status { get; set; }
        public bool Archived { get; set; }
        public DateTime CreateDate { get; set; }
        public long? CreatedBy { get; set; }
    }
}
