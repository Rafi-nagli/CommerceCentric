using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities
{
    public class StyleItemActionHistory
    {
        [Key]
        public long Id { get; set; }
        public long StyleItemId { get; set; }

        public string ActionName { get; set; }
        public string Data { get; set; }
        
        public DateTime CreateDate { get; set; }
        public long? CreatedBy { get; set; }
    }
}
