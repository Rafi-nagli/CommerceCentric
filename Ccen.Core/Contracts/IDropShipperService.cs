using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Contracts
{
    public interface IDropShipperService
    {
        void ConvertStyleToPreorder(int boxType,
            long styleId,
            DateTime when,
            long? by);
    }
}
