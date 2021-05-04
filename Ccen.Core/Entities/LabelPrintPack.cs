using System;
using System.ComponentModel.DataAnnotations;

namespace Amazon.Core.Entities
{
    public class LabelPrintPack
    {
        [Key]
        public long Id { get; set; }
        public string FileName { get; set; }
        public DateTime CreateDate { get; set; }
        public int? NumberOfLabels { get; set; }
        public long? BatchId { get; set; }
        public string PersonName { get; set; }
        public bool IsReturn { get; set; }
    }
}
