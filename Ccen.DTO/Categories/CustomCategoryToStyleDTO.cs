using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Categories
{
    public class CustomCategoryToStyleDTO
    {
        public long Id { get; set; }

        public long CustomCategoryId { get; set; }
        public long? StyleId { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
