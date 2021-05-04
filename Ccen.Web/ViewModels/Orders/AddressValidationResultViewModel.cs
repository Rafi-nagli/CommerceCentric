using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Core.Models.Validation;
using Amazon.DTO;

namespace Amazon.Web.ViewModels.Orders
{
    public class AddressValidationResultViewModel
    {
        public bool IsSuccess { get; set; }
        public IList<CheckResult<AddressDTO>> CheckResults { get; set; }
        public AddressViewModel CorrectedAddress { get; set; }
    }
}