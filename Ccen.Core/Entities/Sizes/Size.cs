using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Amazon.Core.Entities.Enums;

namespace Amazon.Core.Entities.Sizes
{
    public class Size : BaseDateAndByEntity
    {
        [Key]
        public int Id { get; set; }
        public int SizeGroupId { get; set; }
        //public string Title { get; set; }
        public string Name { get; set; }
        public int SortOrder { get; set; }
        public bool DefaultIsChecked { get; set; }

        public string Departments { get; set; }

        public virtual SizeGroup SizeGroup { get; set; }
    }
}
