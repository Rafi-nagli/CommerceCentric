using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models
{
    public enum FillingStyleStatuses
    {
        None = 0,
        Temporary = 10,
        Basic = 20,
        Enhanced = 30,
        AllDataEntered = 40,
        Updated = 50,
        Done = 100
    }

    public static class FillingStyleStatusesHelper
    {
        public static string GetName(FillingStyleStatuses status)
        {
            switch (status)
            {
                case FillingStyleStatuses.None:
                    return "None";
                case FillingStyleStatuses.Temporary:
                    return "Temporary";
                case FillingStyleStatuses.Basic:
                    return "Basic";
                case FillingStyleStatuses.Enhanced:
                    return "Enhanced";
                case FillingStyleStatuses.AllDataEntered:
                    return "AllDataEntered";
                case FillingStyleStatuses.Updated:
                    return "Updated";                
                case FillingStyleStatuses.Done:
                    return "Done";
            }
            return "n/a";
        }
    }
}
