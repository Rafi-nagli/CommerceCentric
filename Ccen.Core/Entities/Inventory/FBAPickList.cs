using Ccen.DTO.Inventory;
using System;
using System.ComponentModel.DataAnnotations;

namespace Amazon.Core.Entities.Inventory
{
    public class FBAPickList
    {
        [Key]
        public long Id { get; set; }

        public ShipmentStatuses Status { get; set; }
        public bool Archived { get; set; }
        public DateTime CreateDate { get; set; }
        public long? CreatedBy { get; set; }
        public string FBAPickListType { get; set; }
    }
}
