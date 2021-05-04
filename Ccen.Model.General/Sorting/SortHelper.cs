using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Entities.Features;
using Amazon.Core.Helpers;
using Amazon.Core.Models;
using Amazon.DTO.Contracts;


namespace Amazon.Model.Implementation.Sorting
{
    public class SortHelper
    {
        public static IList<T> Sort<T>(IList<T> orders, SortMode sortOrder) where T : class, ISortableEntity
        {
            if (sortOrder == SortMode.ByPerson)
            {
                return orders.OrderBy(p => p.PersonName).ToList();
            }

            if (sortOrder == SortMode.ByLocation)
            {
                return OrderByLocation(orders);
            }

            if (sortOrder == SortMode.ByShippingMethodThenLocation)
            {
                return OrderByShippingThenLocation(orders);
            }

            return orders;
        }

        private static IList<T> OrderByLocation<T>(IList<T> orders) where T : ISortableEntity
        {
            //К3, К2, К1 (sorting order – isle, section, shelf, alphabetically product name). 
            //alphabetically нужно так как могут быть несколько стайлов на одной полке
            return orders.OrderBy(o => o.SortIsle)
                .ThenBy(o => o.SortSection)
                .ThenBy(o => o.SortShelf)
                .ThenBy(o => o.SortStyleString)
                .ThenBy(o => SizeHelper.GetSizeIndex(o.SortSize))
                .ThenBy(o => o.SortSize) //NOTE: in case when index equals: XS-2T and 2XL-7Y
                .ThenBy(o => o.SortColor)
                .ThenBy(o => o.FirstItemName)
                .ThenBy(o => o.SortOrderId).ToList();
        }

        private static IList<T> OrderByShippingThenLocation<T>(IList<T> orders) where T : ISortableEntity
        {
            //К3, К2, К1 (sorting order – isle, section, shelf, alphabetically product name). 
            //alphabetically нужно так как могут быть несколько стайлов на одной полке
            return orders.OrderByDescending(o => ShippingUtils.GetShippingMethodIndex(o.ShippingMethodId))
                .ThenBy(o => o.SortIsle)
                .ThenBy(o => o.SortSection)
                .ThenBy(o => o.SortShelf)
                .ThenBy(o => o.SortStyleString)
                .ThenBy(o => SizeHelper.GetSizeIndex(o.SortSize))
                .ThenBy(o => o.SortSize)
                .ThenBy(o => o.SortColor)
                .ThenBy(o => o.FirstItemName)
                .ThenBy(o => o.SortOrderId).ToList();
        }

        public static decimal GetStringIndex(string text)
        {
            var index = 0M;
            if (!String.IsNullOrEmpty(text))
            {
                foreach (var ch in text)
                {
                    index += ch;
                }
            }
            return index;
        }
    }
}