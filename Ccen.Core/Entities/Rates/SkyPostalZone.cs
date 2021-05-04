using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.Rates
{
    public class SkyPostalZone
    {
        [Key]
        public int Id { get; set; }
        public int ServiceType { get; set; }        
        public int StartZip { get; set; }
        public int EndZip { get; set; }  
        public int ZoneId { get; set; }
    }
}
