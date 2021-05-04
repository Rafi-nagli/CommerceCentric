using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Remote
{
    public class RemoteOrdersStatus
    {
        public string Name { get; set; }
        public bool IsSuccess { get; set; }

        public int ToCloseoutCount { get; set; }
        public decimal ToCloseoutWeight { get; set; }
        public IList<string> ToCloseoutIds { get; set; }

        public int OrdersCount { get; set; }
        public decimal OrdersWeight { get; set; }
    }
}
