using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.Listings
{
    public class ParentItemImage : BaseDateAndByEntity
    {
        [Key]
        public long Id { get; set; }
        public long ParentItemId { get; set; }
        public int ImageType { get; set; }
        public string Image { get; set; }
        public int UpdateFailAttempts { get; set; }
    }
}
