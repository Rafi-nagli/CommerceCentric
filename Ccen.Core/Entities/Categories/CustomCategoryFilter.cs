using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.Categories
{
    public class CustomCategoryFilter
    {
        [Key]
        public long Id { get; set; }
        public long CustomCategoryId { get; set; }
        public string AttributeName { get; set; }
        public string Operation { get; set; }
        public string AttributeValues { get; set; }

        public DateTime CreateDate { get; set; }
    }
}
