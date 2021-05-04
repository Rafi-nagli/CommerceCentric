using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.Listings
{
    public class ItemImage : BaseDateAndByEntity
    {
        [Key]
        public long Id { get; set; }
        public long ItemId { get; set; }
        public int ImageType { get; set; }
        public string Image { get; set; }
        public string Name { get; set; }
        public int UpdateFailAttempts { get; set; }

        public decimal? DiffWithLocalImageValue { get; set; }
        public DateTime? DiffWithLocalImageUpdateDate { get; set; }
        public string ComparedLocalImage { get; set; }

        public decimal? DiffWithStyleImageValue { get; set; } 
        public DateTime? DiffWithStyleImageUpdateDate { get; set; }
        public string ComparedStyleImage { get; set; }
    }
}
