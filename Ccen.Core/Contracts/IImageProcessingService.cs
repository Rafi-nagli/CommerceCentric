using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Contracts
{
    public interface IImageProcessingService
    {
        decimal GetDifferences(string imageUrl1,
            string imageUrl2,
            int threshold,
            bool onlyColorDistance);
    }
}
