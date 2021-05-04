using System.ComponentModel.DataAnnotations;

namespace Amazon.Core.Entities
{
    public class OrderComment : BaseDateAndByEntity
    {
        [Key]
        public long Id { get; set; }
        public long OrderId { get; set; }
        [MaxLength(2048)]
        public string Message { get; set; }

        public int Type { get; set; }

        public long? LinkedEmailId { get; set; }

        public string Tag { get; set; }

        public bool Deleted { get; set; }
    }
}
