using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core.Models;

namespace Amazon.Common.Helpers
{
    public class BatchTimeStatusHelper
    {
        public static string GetStatusName(BatchTimeStatus status)
        {
            switch (status)
            {
                case BatchTimeStatus.BeforeFirst:
                    return "Before First";
                case BatchTimeStatus.AfterFirstBeforeSecond:
                    return "Between First - Second";
                case BatchTimeStatus.AfterSecond:
                    return "After Second";
            }
            return "-";
        }
    }
}
