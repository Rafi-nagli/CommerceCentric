using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.Rates
{
    public class DhlGroundZipCode
    {
        [Key]
        public long Id { get; set; }

        public string Zip { get; set; }
    }
}
