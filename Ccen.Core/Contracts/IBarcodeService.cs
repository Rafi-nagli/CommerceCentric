using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.DTO;

namespace Amazon.Core.Contracts
{
    public interface IBarcodeService
    {
        bool RemoveAttachedSKU(long id);

        CustomBarcodeDTO AssociateBarcodes(string sku,
            DateTime when,
            long? by);
    }
}
