using Amazon.Core.Contracts.SystemActions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models.SystemActions.SystemActionDatas
{
    public class UpdateProductBarcodeInput : ISystemActionInput
    {
        public string NewBarcode { get; set; }
    }
}
