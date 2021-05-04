using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.Sizes
{
    public class SizeGroup : BaseDateAndByEntity
    {
        [Key]
        public int Id { get; set; }
        //public string Title { get; set; }
        public string Name { get; set; }

        public string Departments { get; set; }

        public int SortOrder { get; set; }
    }
}
