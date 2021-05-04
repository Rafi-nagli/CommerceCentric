using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models
{
    public enum BoxTypes
    {
        General = 0,
        Preorder = 1
    }

    public class BoxTypesHelper
    {
        public static string TypeToString(BoxTypes type)
        {
            switch (type)
            {
                case BoxTypes.General:
                    return "Received";
                case BoxTypes.Preorder:
                    return "Preorder";
            }
            return "-";
        }
    }
}
