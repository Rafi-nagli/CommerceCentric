using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO
{
    public class BuyerDTO
    {
        public long Id { get; set; }

        public string Email { get; set; }
        public string Name { get; set; }

        public bool InBlackList { get; set; }
        public string InBlackListReason { get; set; }
        public string InBlackListOrderNumber { get; set; }
        public DateTime? InBlackListDate { get; set; }
        public long? InBlackListBy { get; set; }

        public bool RemoveSignConfirmation { get; set; }
        public DateTime? RemoveSignConfirmationDate { get; set; }


        public DateTime? CreateDate { get; set; }
    }
}
