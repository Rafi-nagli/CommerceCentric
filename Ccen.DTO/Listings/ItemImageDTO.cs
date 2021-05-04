using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Listings
{
    public class ItemImageDTO
    {
        public long Id { get; set; }
        public long ItemId { get; set; }
        public string Image { get; set; }
        public int ImageType { get; set; }
        public int UpdateFailAttempts { get; set; }
        public DateTime? UpdateDate { get; set; }
    }
}
