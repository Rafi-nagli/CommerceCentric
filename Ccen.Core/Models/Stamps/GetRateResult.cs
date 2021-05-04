using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.DTO;

namespace Amazon.Core.Models.Stamps
{
    public class GetRateResult
    {
        public IList<RateDTO> Rates { get; set; }
        public GetRateResultType Result { get; set; }
        public string Message { get; set; }

    }
}
