using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities
{
    public class Buyer : BaseDateAndByEntity
    {
        [Key]
        public long Id { get; set; }

        public string Email { get; set; }
        public string Name { get; set; }

        public DateTime? LastOrderDate { get; set; }

        public bool InBlackList { get; set; }
        public string InBlackListReason { get; set; }
        public string InBlackListOrderNumber { get; set; }
        public DateTime? InBlackListDate { get; set; }
        public long? InBlackListBy { get; set; }
        
        public bool RemoveSignConfirmation { get; set; }
        public DateTime? RemoveSignConfirmationDate { get; set; }
    }
}
