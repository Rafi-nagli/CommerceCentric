using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO
{
    public class SizeGroupDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Departments { get; set; }
        
        public int SortOrder { get; set; }
    }
}
