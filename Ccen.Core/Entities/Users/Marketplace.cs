using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities
{
    public class Marketplace
    {
        [Key]
        public int Id { get; set; }
        public long CompanyId { get; set; }
        public string Name { get; set; }
        public int Market { get; set; }
        public string MarketplaceId { get; set; }
        public string Key1 { get; set; }
        public string Key2 { get; set; }
        public string Key3 { get; set; }
        public string Key4 { get; set; }
        public string Key5 { get; set; }
        public string Token { get; set; }
        public string SellerId { get; set; }
        public string EndPointUrl { get; set; }

        public string StoreLogo { get; set; }
        public string StoreUrl { get; set; }
        public string DisplayName { get; set; }
        public string PackingSlipFooterTemplate { get; set; }
        public string TemplateFolder { get; set; }

        public bool IsHidden { get; set; }
        public bool IsActive { get; set; }
        public int SortOrder { get; set; }
    }
}
