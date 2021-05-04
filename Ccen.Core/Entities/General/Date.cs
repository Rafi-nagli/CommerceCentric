using System;

namespace Amazon.Core.Entities
{
    public class Date
    {
        public int Id { get; set; }
        public DateTime BizDate { get; set; }
        public int DateOrder { get; set; }
        public bool Deleted { get; set; }
    }
}
