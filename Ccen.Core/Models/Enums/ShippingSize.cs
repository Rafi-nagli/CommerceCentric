using System.ComponentModel.DataAnnotations;

namespace Amazon.Core.Entities.Enums
{
    public class ShippingSize
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
