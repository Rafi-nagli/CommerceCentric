using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Model.Implementation.Addresses
{
    public class PersonatorAddressCheckResult
    {
        public bool IsSuccess { get; set; }
        public bool IsNotServedByUSPSNote { get; set; }
    }
}
