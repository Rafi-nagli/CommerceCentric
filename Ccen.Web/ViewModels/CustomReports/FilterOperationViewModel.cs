using Amazon.DTO.CustomReports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ccen.Web.ViewModels.CustomReports
{ 
    public class FilterOperationViewModel
    {
        public FilterOperation Operation { get; set; }
        public string OperationString => Operation.ToString();

        public FilterOperationViewModel(FilterOperation operation)
        {
            Operation = operation;
        }
    }
}