using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Listings
{
    public class CategoryDTO
    {
        public int? CategoryId { get; set; }
        public int ParentCategoryId { get; set; }

        public string Name { get; set; }
        public int? Position { get; set; }
        public int? Level { get; set; }
        public int IsActive { get; set; }

        public IList<CategoryDTO> Childs { get; set; }
    }
}
