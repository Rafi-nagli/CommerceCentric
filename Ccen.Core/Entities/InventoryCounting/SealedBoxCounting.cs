using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Amazon.Core.Entities.Inventory
{
    public class SealedBoxCounting : BaseDateAndByEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public long Id { get; set; }
        public long StyleId { get; set; }
        
        public int BoxQuantity { get; set; }
        public int BatchTimeStatus { get; set; }
        public string CountByName { get; set; }

        public DateTime CountingDate { get; set; }

        public int? ApproveStatus { get; set; }
        public DateTime? ApproveStatusDate { get; set; }
        public long? ApproveStatusBy { get; set; }

        public bool IsProcessed { get; set; }
    }
}
