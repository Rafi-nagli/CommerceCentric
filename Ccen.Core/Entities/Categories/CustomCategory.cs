using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.Categories
{
    public class CustomCategory
    {
        [Key]
        public long Id { get; set; }
        public string Name { get; set; }
        public int? CategoryId { get; set; }
        public string ParentCategoryName { get; set; }
        public string CategoryPath { get; set; }

        public int Mode { get; set; }
        
        public string ShopifyUrl { get; set; }
        public string ShopifyName { get; set; }
        public string ShopifyDescription { get; set; }


        public int Market { get; set; }
        public string MarketplaceId { get; set; }

        public DateTime CreateDate { get; set; }
    }
}
