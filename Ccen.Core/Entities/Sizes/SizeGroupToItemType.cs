using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Amazon.Core.Entities.Enums;

namespace Amazon.Core.Entities.Sizes
{
    public class SizeGroupToItemType
    {
        [Key]
        public int Id { get; set; }
        public int SizeGroupId { get; set; }
        //public string Title { get; set; }
        public int ItemTypeId { get; set; }
        public DateTime? CreateDate { get; set; }
        public long? CreatedBy { get; set; }
    }
}
