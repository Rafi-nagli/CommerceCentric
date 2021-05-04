using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core;

namespace Amazon.Common.Helpers
{
    public class SkuHelper
    {
        public static bool IsPrime(string sku)
        {
            if (string.IsNullOrEmpty(sku))
                return false;
            if (sku.Contains("_FBP") || sku.Contains("-FBP"))
                return true;
            return false;
        }

        public static string AddPrimePostFix(string sku)
        {
            var parts = sku.Split("-".ToCharArray()).ToList();
            if (parts.Count > 1)
                parts.Insert(parts.Count - 1, "FBP");
            else
                parts.Add("FBP");

            return String.Join("-", parts);
        }

        public static string PrepareSKU(string sku)
        {
            return StringHelper.KeepOnlyAlphanumeric(StringHelper.RemoveWhitespace(sku), new char[] { '-', '_' }, "").Replace("--", "-").Replace("--", "-").Replace("/", "");
        }

        public static string RemoveSKULastIndex(string sku)
        {
            var parts = sku.Split("-".ToCharArray());
            if (parts.Length > 1 && StringHelper.TryGetInt(parts[parts.Length - 1]).HasValue)
                parts = parts.Take(parts.Length - 1).ToArray();
            return String.Join("-", parts);
        }

        public static string RemoveSKULastPartNames(string sku, IList<string> partNames)
        {
            if (partNames == null
                || !partNames.Any())
                return null;

            var parts = sku.Split("-".ToCharArray());
            if (parts.Length > 1 && StringHelper.EqualWithOneOfStrings(parts.Last(), partNames))
                parts = parts.Take(parts.Length - 1).ToArray();

            return String.Join("-", parts);
        }

        public static string RemoveSKUMiddleIndex(string sku)
        {
            var parts = sku.Split("-".ToCharArray()).ToList();
            var index = 1;
            while (index < parts.Count - 1) //Exclude first and last part
            {
                var middelIndex = StringHelper.TryGetInt(parts[index]);
                if (middelIndex.HasValue
                    && middelIndex.Value < 15)
                {
                    parts.RemoveAt(index);
                    break;
                }
            }
            return String.Join("-", parts);
        } 

        public static string SetSKUMiddleIndex(string sku, int index)
        {
            if (index <= 0)
                return sku;

            if (index == 1) //NOTE: Skipped index = 1, start with 2
                index = 2;

            var parts = sku.Split("-".ToCharArray()).ToList();
            if (parts.Count > 1)
                parts.Insert(parts.Count - 1, index.ToString());
            else
                parts.Add(index.ToString());

            return String.Join("-", parts);
        }

        public static string RetrieveSizeFromSKU(string sku)
        {
            if (String.IsNullOrEmpty(sku))
                return null;
            var parts = sku.Split(new[] { '-', '_', '=', ' ' });
            if (parts.Length > 1)
                return parts.Last();
            return null;
        }

        public static string RetrieveStyleIdFromSKU(IUnitOfWork db, string sku, string name)
        {
            if (String.IsNullOrEmpty(sku))
                return sku;

            //Obsolet
            //1. search for '-%size%'
            //2. "убераем всйо после первого дефиса"
            //NOTE: для этой компании, "saras print", а дальше будем смотреть
            //NOTE: not it helps to resolve style for ex: 21WN005EZSJC-1-4T, remove all postfixs
            //End obsolet


            //Last part after "-" = Size
            //First part style name
            //Midle part color or/and index, index always should be removed

            var indexOfStar = sku.IndexOf("*", StringComparison.Ordinal);
            if (indexOfStar > 0)
            {
                var indexOfNextSeparator = sku.IndexOfAny(new[] { '-' }, indexOfStar);//, '_', '=', ' ' }
                if (indexOfNextSeparator == -1)
                    indexOfNextSeparator = sku.Length;
                sku = sku.Remove(indexOfStar, indexOfNextSeparator - indexOfStar);
            }

            //ex: 21WN005EZSJC-YNY-2-4T => 21WN005EZSJC-YNY
            //ex: 21WN005EZSJC-4T => 21WN005EZSJC
            //ex: 21WN005EZSJC-2 => 21WN005EZSJC
            //ex: 21WN005EZSJC-YNY => 21WN005EZSJC-YNY
            //ex: K157782BM-5-FBA-2T => K157782BM
            //ex: 1557-SMB-2-5
            //ex: AP-1852-DOG-4T
            //ex: FEB17SWIMLI1BMINI-2T-3T

            var parts = sku.Split(new[] { '-' }); //NOTE: '_' - not a part delimeter (exceptions: FASHIONMASK_2PK-BLACK), other too: '_', '=', ' ' });
            var style = parts[0];

            for (int i = parts.Length - 1; i > 0; i--) //NOTE: i > 1, always keep first part
            {
                var part = parts[i];

                //Skip Int postfix
                var intValue = StringHelper.TryGetInt(part);
                if (intValue != null && part.Length < 3) //Exclude: ex: WF17351-100-2T
                {
                    continue;
                }
                
                if (part.ToUpper() == "FBP"
                    || part.ToUpper() == "FBA")
                {
                    continue;
                }

                //NOTE: check with sizes only last one (or combination with two last parts)
                //ex: FEB17SWIMLI1BMINI-2T-3T
                if (i == parts.Length - 1 || i == parts.Length - 2)
                {
                    var size = part;
                    if (i == parts.Length - 2)
                        size = part + "-" + parts.Last();
                    var isSize = db.SizeMappings.IsSize(size) || db.Sizes.IsSize(size);
                    if (!isSize)
                    {
                        //Check if last part of combined size: ex: FEB17SWIMLI1BMINI-2T-3T
                        if (i == parts.Length - 1 && parts.Length > 2)
                        {
                            size = parts[parts.Length - 2] + "-" + part;
                            isSize = db.SizeMappings.IsSize(size) || db.Sizes.IsSize(size);
                        }
                    }

                    if (isSize)
                    {
                        continue;
                    }
                }

                style = String.Join("-", parts.Take(i + 1));
                break;
            }

            return style;
        }
    }
}
