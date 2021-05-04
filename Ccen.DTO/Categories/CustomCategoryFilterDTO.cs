using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Categories
{
    public class CustomCategoryFilterDTO
    {
        public long Id { get; set; }
        public long CustomCategoryId { get; set; }
        public string AttributeName { get; set; }
        public string Operation { get; set; }
        public string AttributeValues { get; set; }

        public DateTime CreateDate { get; set; }
    }
}
