using System.ComponentModel.DataAnnotations;

namespace Amazon.Core.Entities
{
    public class Push
    {
        [Key]
        public int Id { get; set; }
        public string DeviceId { get; set; }
        public string RegistrationId { get; set; }
    }
}
