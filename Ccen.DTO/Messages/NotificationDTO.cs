using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Messages
{
    public class NotificationDTO
    {
        public long Id { get; set; }

        public int Type { get; set; }

        public string RelatedEntityId { get; set; }
        public int RelatedEntityType { get; set; }

        public string Message { get; set; }
        public string Tag { get; set; }
        public string AdditionalParams { get; set; }

        public bool IsRead { get; set; }
        public DateTime? ReadDate { get; set; }
        public long? ReadBy { get; set; }

        public DateTime? CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
    }
}
