using System.ComponentModel.DataAnnotations;

namespace Amazon.Core.Entities
{
    public class ProductComment : BaseDateAndByEntity
    {
        [Key]
        public long Id { get; set; }
        [MaxLength(2048)]
        public string Message { get; set; }

        public int ProductId { get; set; }
        public bool Deleted { get; set; }

    }
}
