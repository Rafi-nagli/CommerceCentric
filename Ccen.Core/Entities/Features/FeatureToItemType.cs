using System;
using System.ComponentModel.DataAnnotations;
using Amazon.Core.Entities.Enums;

namespace Amazon.Core.Entities.Features
{
    public class FeatureToItemType
    {
        [Key]
        public int Id { get; set; }
        public int FeatureId { get; set; }
        public int ItemTypeId { get; set; }
        public DateTime? CreateDate { get; set; }
        public int? CreatedBy { get; set; }
    }
}
