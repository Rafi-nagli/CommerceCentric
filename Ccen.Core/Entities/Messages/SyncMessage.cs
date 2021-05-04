using System;
using System.ComponentModel.DataAnnotations;

namespace Amazon.Core.Entities
{
    public class SyncMessage
    {
        [Key]
        public long Id { get; set; }
        public long SyncHistoryId { get; set; }
        public string EntityId { get; set; }
        public string Message { get; set; }
        public int Status { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
