using System;
using System.ComponentModel.DataAnnotations;

namespace Amazon.Core.Entities.Features
{
    public class StyleFeatureValue : BaseDateAndByEntity
    {
        [Key]
        public int Id { get; set; }

        public long StyleId { get; set; }
        public int FeatureId { get; set; }
        public int FeatureValueId { get; set; }
    }
}
