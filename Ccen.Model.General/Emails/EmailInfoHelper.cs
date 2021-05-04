using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Common.Helpers;
using Amazon.DTO;
using Amazon.DTO.Listings;

namespace Amazon.Model.Models
{
    public class EmailInfoHelper
    {
        public static string GetDateString(DateTime? date)
        {
            return DateHelper.ToDateString(date);
        }

        public static string GetProductString(IList<ListingOrderDTO> items, string separator)
        {
            return string.Join(separator, items.Where(i => i.QuantityOrdered > 0).Select(i => GetProductQuantityString(i.QuantityOrdered) + "\"" + i.Title + "\"" + " - " + i.Size));
        }

        public static string GetProductQuantityString(int quantity)
        {
            if (quantity < 2)
                return "";
            return StringHelper.ConvertToWordNumber(quantity) + " ";
        }

        public static string GetShortProductString(IList<string> itemNames)
        {
            if (itemNames == null || !itemNames.Any())
            {
                return string.Empty;
            }
            var number = itemNames.Count();
            switch (number)
            {
                case 1:
                    return "\"" + itemNames.First() + "\"";
                case 2:
                    return "\"" + itemNames.First() + "\" & \"" + itemNames.Last() + "\" Items";
                default:
                    return number + " Items";
            }
        }

        public static string GetOrdersString(IList<string> orderNumbers)
        {
            return string.Join(", ", orderNumbers);
        }

        public static string GetFirstName(string name)
        {
            if (!String.IsNullOrEmpty(name))
            {
                return name.Split(' ').First();
            }
            return String.Empty;
        }

        public static string GetLastName(string name)
        {
            if (!String.IsNullOrEmpty(name))
            {
                return name.Split(' ').Last();
            }
            return String.Empty;
        }
    }
}
