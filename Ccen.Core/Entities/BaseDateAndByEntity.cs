using System;

namespace Amazon.Core.Entities
{
    public class BaseDateAndByEntity
    {
        public DateTime? UpdateDate { get; set; }
        public long? UpdatedBy { get; set; }
        public DateTime? CreateDate { get; set; }
        public long? CreatedBy { get; set; }
    }
}
