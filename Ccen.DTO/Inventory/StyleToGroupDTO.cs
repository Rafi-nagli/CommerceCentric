using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Inventory
{
    public class StyleToGroupDTO
    {
        public long Id { get; set; }
        public long StyleGroupId { get; set; }
        public long StyleId { get; set; }        

        public DateTime CreateDate { get; set; }
        public long? CreatedBy { get; set; }

        //Additional
        public string StyleString { get; set; }
    }
}
