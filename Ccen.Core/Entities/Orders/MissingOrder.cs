using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Amazon.Core.Entities
{
    public class SystemMessage
    {
        [Key]
        public long Id { get; set; }
        public string Name { get; set; }
        public string Tag { get; set; }
        public string Message { get; set; }
        public string Data { get; set; }
        public int? Status { get; set; }
        public DateTime? UpdateDate { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
