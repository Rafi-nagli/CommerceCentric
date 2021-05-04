using Amazon.DTO.DropShippers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Contracts
{
    public interface IDSService
    {
        IList<DropShipperDTO> GetAll();
        string GetNameById(int? dsId);
    }
}
