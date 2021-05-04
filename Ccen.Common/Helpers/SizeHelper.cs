using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Common.Helpers;
using Amazon.DTO.Listings;
using Amazon.Utils;

namespace Amazon.Core.Helpers
{
    public class SizeHelper
    {
        public static string SizeCorrection(string size, string styleSize)
        {
            if (size == "X-Small"
                || size == "XSmall"
                || size == "XS"

                || size == "Small"
                || size == "S"

                || size == "Medium"
                || size == "M"

                || size == "Large"
                || size == "L"

                || size == "X-Large"
                || size == "XLarge"
                || size == "XL"

                || size == "2X-Large"
                || size == "2XLarge"
                || size == "XXLarge"
                || size == "2XL"
                || size == "XXL"

                || size == "3X-Large"
                || size == "3XLarge"
                || size == "XXXLarge"
                || size == "3XL"
                || size == "XXXL")
            {
                var hasDisit = SizeHelper.GetFirstNumber(styleSize) > 0;
                if (hasDisit)
                    return size + " / " + styleSize.Replace("/", "-");
            }
            return size;
        }

        public static string UIFormatSize(string size)
        {
            if (String.IsNullOrEmpty(size))
                return size;

            var digit = new String(size.Where(Char.IsDigit).ToArray());
            if (digit.Length > 0)
            {
                long dSize = 0;
                if (long.TryParse(digit, out dSize))
                {
                    if (dSize == 6)
                        return size + " [six]";
                    if (dSize == 8)
                        return size + " [eight]";
                }
            }
            return size;
        }

        public static bool IsNumXSmallSize(string size)
        {
            if (size == "4"
                || size == "4/5")
                return true;
            return false;
        }

        public static bool IsNumSmallSize(string size)
        {
            if (size == "6/6x"
                || size == "6/7"
                || size == "6")
                return true;
            return false;
        }

        public static bool IsNumMediumSize(string size)
        {
            if (size == "7/8"
                || size == "8")
                return true;
            return false;
        }

        public static bool IsNumLargeSize(string size)
        {
            if (size == "10"
                || size == "10/12")
                return true;
            return false;
        }

        public static bool IsNumXLargeSize(string size)
        {
            if (size == "12/14"
                || size == "14"
                || size == "14/16"
                || size == "16")
                return true;
            return false;
        }

        public static bool IsChildAge(string size)
        {
            if (IsToddlers(size))
                return true;

            var firstDigits = StringHelper.GetFirstDigitSequences(size);
            if (firstDigits <= 6)
                return true;
            return false;
        }

        public static bool IsTweenAge(string size)
        {
            if (IsToddlers(size))
                return false;

            var firstDigits = StringHelper.GetFirstDigitSequences(size);
            if (firstDigits >= 2 && firstDigits <= 16)
                return true;
            return false;
        }

        public static bool IsTeenAge(string size)
        {
            var firstDigits = StringHelper.GetFirstDigitSequences(size);
            if (firstDigits >= 7 && firstDigits <= 16)
                return true;
            return false;
        }

        public static bool IsAdultAge(string gender)
        {
            gender = (gender ?? "").ToLower();
            if (gender == "womens" || gender == "mens")
                return true;
            return false;
        }

        public static bool IsToddlers(string size)
        {
            if (String.IsNullOrEmpty(size))
                return false;
            size = size.ToLower();
            if (size == "2t"
                || size == "2t/3t"
                || size == "3t"
                || size == "4t"
                || size == "5t")
            {
                return true;
            }
            if (size.Contains("M") && (size.Contains("12") || size.Contains("18") || size.Contains("24"))
                || size.Contains("Month")
                || size.Contains("month"))
            {
                return true;
            }

            return false;
        }


        public static string PrepareSizeForExport(IUnitOfWork db, string size, IList<ItemSizeMapping> existSizeMappings)
        {
            var existSizes = existSizeMappings.Select(s => s.ItemSize).ToList();
            var hasNumbericItemSize = existSizes.Any(s => IsNumXSmallSize(s)
                                                         || IsNumSmallSize(s)
                                                         || IsNumMediumSize(s)
                                                         || IsNumLargeSize(s)
                                                         || IsNumXLargeSize(s));

            var sizeMappings = db.SizeMappings.GetAllAsDto().Where(s => s.StyleSize == size).ToList();
            foreach (var sizeMapping in sizeMappings)
            {
                if (existSizes.Contains(sizeMapping.ItemSize))
                    return sizeMapping.ItemSize;
            }

            //NOTE: when already has numberic item size prevent converting to S/M/L
            if (!hasNumbericItemSize)
            {
                if (IsNumXLargeSize(size))
                    return "X-Large";
                if (IsNumLargeSize(size))
                    return "Large";
                if (IsNumMediumSize(size))
                    return "Medium";
                if (IsNumSmallSize(size))
                    return "Small";
                if (IsNumXSmallSize(size))
                    return "X-Small";
            }

            return size;
        }

        public static string PrepareSizeForSKU(string size, bool forceReplace)
        {
            if (String.IsNullOrEmpty(size))
                return size;

            if (size.IndexOf("/", StringComparison.Ordinal) >= 0 || forceReplace)
            {
                if (IsNumXSmallSize(size))
                    return "XS";
                if (IsNumSmallSize(size))
                    return "S";
                if (IsNumMediumSize(size))
                    return "M";
                if (IsNumLargeSize(size))
                    return "L";
                if (IsNumXLargeSize(size))
                    return "XL";
            }
            return size.Replace("-", "");
        }

        public static string ToFullVariation(string size, string color)
        {
            var variation = "";
            if (String.IsNullOrEmpty(size))
                variation += "-";
            else
                variation += size;
            if (String.IsNullOrEmpty(color))
                variation += "/-";
            else
                variation += "/" + color;
            return variation;
        }

        public static string ToVariation(string size, string color)
        {
            var variation = "";
            if (String.IsNullOrEmpty(size))
                variation += "-";
            else
                variation += size;
            if (String.IsNullOrEmpty(color))
                variation += "";
            else
                variation += "/" + color;
            return variation;
        }


        public static bool IsDigitSize(string size)
        {
            return size.Where(c => c != '/' || c != '\\').All(Char.IsDigit);
        }


        public static float IntSize(string size)
        {
            try
            {
                int result;
                if (Int32.TryParse(size, out result))
                {
                    return result;
                }

                size = size.ToLower();
                size = size.Replace("x-small", "1").Replace("xs", "1");
                size = size.Replace("small", "2");
                size = size.Replace("medium", "3");
                size = size.Replace("large", "4");

                var digit = new String(size.Where(Char.IsDigit).ToArray());
                float value = digit.Length > 0 ? GeneralUtils.TryGetInt(digit, 0) : 0;

                //Months sizes should going ahead
                if (size.ToLower().Contains("month"))
                    value = value / (float)100;

                return value;
            }
            catch { return 0; }
        }

        public static int GetFirstNumber(string text)
        {
            try
            {
                int result;
                if (Int32.TryParse(text, out result))
                {
                    return result;
                }

                var digit = String.Empty;
                for (int i = 0; i < text.Length; i++)
                {
                    if (Char.IsDigit(text[i]))
                        digit += text[i];
                    else if (digit.Length > 0)
                        break;
                }

                return digit.Length > 0
                    ? Int32.Parse(digit)
                    : 0;
            }
            catch { return 0; }
        }

        public static int GetLastNumber(string text)
        {
            try
            {
                int result;
                if (Int32.TryParse(text, out result))
                {
                    return result;
                }

                var digit = String.Empty;
                for (int i = text.Length - 1; i >= 0; i--)
                {
                    if (Char.IsDigit(text[i]))
                        digit += text[i];
                    else if (digit.Length > 0)
                        break;
                }
                digit = new string(digit.Reverse().ToArray());

                return digit.Length > 0
                    ? Int32.Parse(digit)
                    : 0;
            }
            catch { return 0; }
        }

        public static int GetMaxNumber(string text)
        {
            try
            {
                int result;
                if (Int32.TryParse(text, out result))
                {
                    return result;
                }

                var digit = String.Empty;
                int? maxNumber = null;
                for (int i = 0; i < text.Length; i++)
                {
                    if (Char.IsDigit(text[i]))
                        digit += text[i];
                    else if (digit.Length > 0)
                    {
                        var newNum = StringHelper.TryGetInt(digit);
                        if (newNum.HasValue
                                && (newNum > maxNumber || maxNumber == null))
                        {
                            maxNumber = newNum;
                            digit = "";
                        }
                    }      
                }

                //With digit in the end
                if (digit.Length > 0)
                {
                    var newNum = StringHelper.TryGetInt(digit);
                    if (newNum.HasValue
                            && (newNum > maxNumber || maxNumber == null))
                    {
                        maxNumber = newNum;
                    }
                }

                return maxNumber ?? 0;
            }
            catch { return 0; }
        }


        public static float IntSizeFirstDigitSequences(string size)
        {
            try
            {
                //Find first digit sequences
                String digit = String.Empty;
                for (int i = 0; i < size.Length; i++)
                {
                    if (Char.IsDigit(size[i]))
                        digit += size[i];
                    else if (digit.Length > 0)
                        break;
                }

                float value = 0;
                if (digit.Length > 0)
                    value = GeneralUtils.TryGetLong(digit, 0);

                //NOTE: for 7/8, 6/6x, 6/7, e.t.c
                if (value < 20)
                {
                    if (size.Contains("/"))
                        value += 0.5f;
                }

                return value;
            }
            catch { }
            return 0;
        }





        public static string FormatSize(string size)
        {
            if (String.IsNullOrEmpty(size))
                return size;

            if (size.Contains("X-Small") || size.Contains("XSmall"))
                return "XS";
            if (size.Contains("Small"))
                return "S";
            if (size.Contains("Medium"))
                return "M";
            if (size.Contains("X-Large") || size.Contains("XLarge")) //X-Large should be upper then Large (more specific creteria)
                return "XL";
            if (size.Contains("Large"))
                return "L";
            if (size.Contains("XX-Large") || size.Contains("XXLarge"))
                return "2X";

            return size;
        }

        public static float GetSizeIndex(string size)
        {
            if (String.IsNullOrEmpty(size))
                return 0;

            var lSize = size.ToLower();

            //Invert order
            if (lSize.Contains("4xl") || lSize.StartsWith("xxxxl-"))
                return 1.108f;
            if (lSize.Contains("xl/xxxl") || lSize.Contains("xl/3x") || lSize.Contains("3xl") || lSize.StartsWith("xxxl-"))
                return 1.107f;
            if (lSize.Contains("xx-large") || lSize.Contains("xxlarge") || lSize == "xxl" || lSize == "2x" || lSize.Contains("2xl")
                || lSize.StartsWith("xxl-"))
                return 1.106f;
            if (lSize.Contains("xl/xxl") || lSize.Contains("xl/2x"))
                return 1.1055f;
            if (lSize.Contains("x-large") || lSize.Contains("xlarge") || lSize.StartsWith("xl"))
                return 1.105f;
            if (lSize.Contains("l/xl"))
                return 1.1045f;
            if (lSize.Contains("large") || lSize == "l" || lSize.StartsWith("l-"))
                return 1.104f;
            if (lSize.Contains("m/l"))
                return 1.1035f;
            if (lSize.Contains("medium") || lSize == "m" || lSize.StartsWith("m-"))
                return 1.103f;
            if (lSize.Contains("s/m"))
                return 1.1025f;
            if (lSize.Contains("small") || lSize == "s" || lSize.StartsWith("s-"))
                return 1.102f;
            if (lSize.Contains("xs/s"))
                return 1.1015f;
            if (lSize.Contains("x-small") || lSize.Contains("xsmall") || lSize == "xs" || lSize.StartsWith("xs-"))
                return 1.101f;


            if (lSize == "2t")
                return 1.002f;
            if (lSize == "2t/3t")
                return 1.0025f;
            if (lSize == "3t")
                return 1.003f;
            if (lSize == "4t")
                return 1.004f;
            if (lSize == "5t")
                return 1.005f;

            var index = IntSizeFirstDigitSequences(size);

            //Months sizes should going ahead
            if (size.ToLower().Contains("month")
                || size.ToLower() == "12m"
                || size.ToLower() == "18m"
                || size.ToLower() == "24m")
                index = index / (float)100;

            return index;
        }


        public static string ExcludeSizeInfo(string text)
        {
            if (String.IsNullOrEmpty(text))
                return text;

            //Girls' Pajamas and Nightgowns with Matching 18 Inch Doll Sleepwear, Sizes 4-16
            var index = text.IndexOf("Size", StringComparison.OrdinalIgnoreCase);
            if (index > 0)
                return text.Substring(0, index);

            if (text.Length > 14)
            {
                //Peppa Pig Girls Toddler Plush Pink Bathrobe Robe Pajamas (2t/3t)
                //Komar Kids Big Girls' Santa's Workshop Pink Pajamas L/10-12
                //Angry Birds Boys' "Born to be Angry" 2 Piece Long Pajama Set L(10/12)
                //DC Comics Big Boys' Batman Vs Superman Sleepwear Coat Set, Black, Large
                //Widgeon Little Girls' 3 Bow Faux Fur Coat with Hat, color, talla 3
                //Komar Kids Big Boys' Justice League BMJ Coat Set, Black, Large(10/12)
                //Avengers Little Boys Multi Color Character Printed 2pc Pajama Short Set (4)
                //Ever After High Girl's Costume Nightgown (L (10/12))

                //TODO: exclude Komar Kids Girls' Big Girls' Dotty Owl 2pc Sleepwear Set
                //TODO: Finding Dory Nemo Little Girls Flannel Coat Style Pajamas (2T, Bubbles Blue)


                index = IndexOfBest(text + " ", //NOTE: Add space to check "S " keywords
                    text.Length - 14,
                    new string[]
                    {
                        "1",
                        "2",
                        "3",
                        "4",
                        "5",
                        "6",
                        "7",
                        "8",
                        "9",
                        "0",

                        "talla",


                        "XSmall",
                        "X-Small",
                        "Small",
                        "Medium",
                        "Large",
                        "XLarge",
                        "X-Large",
                        "XX-Large",
                        "2XX-Large",

                        "XS-",
                        "S-",
                        "M-",
                        "L-",
                        "XL-",

                        "XS -",
                        "S -",
                        "M -",
                        "L -",
                        "XL -",

                        "XS/",
                        "S/",
                        "M/",
                        "L/",
                        "XL/",
                        
                        "XS /",
                        "S /",
                        "M /",
                        "L /",
                        "XL /",

                        "XS,",
                        "S,",
                        "M,",
                        "L,",
                        "XL,",

                        "XS(",
                        "S(",
                        "M(",
                        "L(",
                        "XL(",

                        "XS (",
                        "S (",
                        "M (",
                        "L (",
                        "XL (",
                    });
                if (index > 0)
                {
                    text = text.Substring(0, index);
                }
                text = TrimEnd(text, new[]
                {
                    " XS",
                    " S",
                    " M",
                    " L",
                    " XL",
                });
                text = text.TrimEnd("() ,_;-".ToCharArray());
            }
            return text;
        }

        public static bool? IsBoy(string gender)
        {
            if (String.IsNullOrEmpty(gender))
                return null;
            return gender.IndexOf("boy", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static bool? IsGirl(string gender)
        {
            if (String.IsNullOrEmpty(gender))
                return null;
            return gender.IndexOf("girl", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static bool? IsMen(string gender)
        {
            if (String.IsNullOrEmpty(gender))
                return null;
            return gender.IndexOf("men", StringComparison.OrdinalIgnoreCase) >= 0
                && gender.IndexOf("women", StringComparison.OrdinalIgnoreCase) == -1;
        }

        public static bool? IsWomen(string gender)
        {
            if (String.IsNullOrEmpty(gender))
                return null;
            return gender.IndexOf("women", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static bool IsUnisex(string gender)
        {
            if (String.IsNullOrEmpty(gender))
                return false;
            return gender.IndexOf("unisex", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string TrimEnd(string source, string[] keywords)
        {
            if (String.IsNullOrEmpty(source))
                return source;

            foreach (var keyword in keywords)
            {
                if (source.EndsWith(keyword, StringComparison.OrdinalIgnoreCase))
                    source = source.Substring(0, source.Length - keyword.Length);
            }
            return source;
        }

        private static int IndexOfBest(string source, int startIndex, string[] keywords)
        {
            var bestIndex = -1;
            foreach (var keyword in keywords)
            {
                var index = source.IndexOf(keyword, startIndex, StringComparison.OrdinalIgnoreCase);
                if (index != -1 && (index < bestIndex || bestIndex == -1))
                    bestIndex = index;
            }
            return bestIndex;
        }


        public static IList<int> GetBreakdowns(List<string> sizes)
        {
            var breakdowns = new List<int>();

            if (sizes.Count < 1)
                return null;

            //b.	If sizes 4,6,8,10 or 4/5, 6/6x, 7/8, 10/12 or 4/5, 6/7, 8, 10/12 1-2-2-1
            if (sizes.Count == 4
                && (sizes.All(s => s == "4"
                               || s == "6"
                               || s == "8"
                               || s == "10")
                || sizes.All(s => s == "4/5"
                                  || s == "6/6x"
                                  || s == "7/8"
                                  || s == "10/12")
                || sizes.All(s => s == "4/5"
                                  || s == "6/7"
                                  || s == "8"
                                  || s == "10/12")
                || sizes.All(s => s == "4/5"
                                  || s == "6"
                                  || s == "7/8"
                                  || s == "10/12")))
            {
                var firstSize = sizes.FirstOrDefault(s => s == "4/5" || s == "4");
                var lastSize = sizes.FirstOrDefault(s => s == "10/12" || s == "10");
                if (firstSize != null && lastSize != null)
                {
                    var otherSizes = sizes.Where(s => s != firstSize && s != lastSize).ToList();
                    if (otherSizes.Count() == 2)
                    {
                        foreach (var size in sizes)
                        {
                            if (size == firstSize || size == lastSize)
                                breakdowns.Add(1);
                            else
                                breakdowns.Add(2);
                        }
                        return breakdowns;
                    }
                }
            }

            if (sizes.Count == 4
                && (sizes.All(s => s == "6"
                               || s == "7/8"
                               || s == "10/12"
                               || s == "14/16")
                || sizes.All(s => s == "6/6x"
                                  || s == "7/8"
                                  || s == "10/12"
                                  || s == "14/16")))
            {
                var firstSize = sizes.FirstOrDefault(s => s == "6" || s == "6/6x");
                var lastSize = sizes.FirstOrDefault(s => s == "14/16");
                if (firstSize != null && lastSize != null)
                {
                    var otherSizes = sizes.Where(s => s != firstSize && s != lastSize).ToList();
                    if (otherSizes.Count() == 2)
                    {
                        foreach (var size in sizes)
                        {
                            if (size == firstSize || size == lastSize)
                                breakdowns.Add(1);
                            else
                                breakdowns.Add(2);
                        }
                        return breakdowns;
                    }
                }
            }

            var sortedSizes = sizes.OrderBy(s => SizeHelper.GetSizeIndex(s)).ToList();
            var minSize = sortedSizes.FirstOrDefault();
            var maxSize = sortedSizes.LastOrDefault();
            //If sizes are 4-[any]-10 or 6-[any]-12, put default breakdown 1-2-2-1.
            if (sizes.Count == 4)
            {
                if ((minSize == "4" && maxSize == "10")
                    || (minSize == "6" && maxSize == "12"))
                {
                    foreach (var size in sizes)
                    {
                        if (size == "4" || size == "6")
                            breakdowns.Add(1);
                        else if (size == "10" || size == "12")
                            breakdowns.Add(1);
                        else
                            breakdowns.Add(2);
                    }
                    return breakdowns;
                }
            }

            //For sizes 4-[any]-8 (just 3 sizes) default breakdown 2-3-1.
            if (sizes.Count == 3)
            {
                //a.	If sizes 2T-4T only, 2-2-2
                if (sizes.All(s => s == "2T"
                                   || s == "3T"
                                   || s == "4T"))
                {
                    breakdowns.Add(2);
                    breakdowns.Add(2);
                    breakdowns.Add(2);
                    return breakdowns;
                }

                if (minSize == "4"
                    && maxSize == "8")
                {
                    foreach (var size in sizes)
                    {
                        if (size == "4")
                            breakdowns.Add(2);
                        else if (size == "8")
                            breakdowns.Add(1);
                        else
                            breakdowns.Add(3);
                    }
                    return breakdowns;
                }

                if (minSize == "4/5"
                    && maxSize == "8")
                {
                    foreach (var size in sizes)
                    {
                        breakdowns.Add(2);
                    }
                    return breakdowns;
                }
            }

            //For 5-sizes 4/5 - 14/16: 1-1-2-1-1
            if (sizes.Count == 5)
            {
                var middleSize = sortedSizes[2];
                if (minSize == "4/5"
                    && maxSize == "14/16")
                {
                    foreach (var size in sizes)
                    {
                        if (size == middleSize)
                            breakdowns.Add(2);
                        else
                            breakdowns.Add(1);
                    }
                    return breakdowns;
                }
            }

            //For 6-sizes 4-16: 1-2-2-2-1-1
            if (sizes.Count == 6)
            {
                var preMaxSize = sortedSizes[4];
                if (minSize == "4"
                    && maxSize == "14")
                {
                    foreach (var size in sizes)
                    {
                        if (size == minSize 
                            || size == maxSize
                            || size == preMaxSize)
                            breakdowns.Add(1);
                        else
                            breakdowns.Add(2);
                    }
                    return breakdowns;
                }
            }

            return null;
        }
    }
}