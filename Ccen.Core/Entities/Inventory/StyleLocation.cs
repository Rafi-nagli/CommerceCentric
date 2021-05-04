using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Amazon.Core.Entities.Inventory
{
    public class StyleLocation : BaseDateAndByEntity
    {
        [Key]
        public long Id { get; set; }
        public long StyleId { get; set; }
        public int SortIsle { get; set; }
        public int SortSection { get; set; }
        public int SortShelf { get; set; }

        public string Isle { get; set; }
        public string Section { get; set; }
        public string Box { get; set; }
        public string Shelf { get; set; }

        public bool IsDefault { get; set; }

    }
}
