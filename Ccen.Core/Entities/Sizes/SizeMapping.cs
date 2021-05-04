using System.ComponentModel.DataAnnotations;

namespace Amazon.Core.Entities
{
    public class SizeMapping : BaseDateAndByEntity
    {
        [Key]
        public int Id { get; set; }

        public string StyleSize { get; set; }
        public string ItemSize { get; set; }

        public int Priority { get; set; }

        public bool IsSystem { get; set; }
    }
}
