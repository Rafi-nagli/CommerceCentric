using System;
using System.ComponentModel.DataAnnotations;

namespace Amazon.Core.Entities
{
    public class USPSHoliday
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Key { get; set; }
        public DateTime Date { get; set; }
    }
}
