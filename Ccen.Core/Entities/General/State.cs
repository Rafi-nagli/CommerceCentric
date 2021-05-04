using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Amazon.Core.Entities
{
    public class State
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int StateId { get; set; }

        public string CountryCode { get; set; }
        public string StateCode { get; set; }
        public string StateName { get; set; }
        public bool IsBase { get; set; }
    }
}
