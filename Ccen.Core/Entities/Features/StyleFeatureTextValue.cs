using System;
using System.ComponentModel.DataAnnotations;
using Amazon.Core.Entities.Inventory;

namespace Amazon.Core.Entities.Features
{
    public class StyleFeatureTextValue : BaseDateAndByEntity
    {
        [Key]
        public int Id { get; set; }
        public int FeatureId { get; set; }
        public long StyleId { get; set; }
        public string Value { get; set; }
    }
}
