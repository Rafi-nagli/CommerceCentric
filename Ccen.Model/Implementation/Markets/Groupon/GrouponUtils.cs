using Amazon.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Model.Implementation.Markets.Groupon
{
    public class GrouponUtils
    {
        public static string PrepareText(string text)
        {
            if (String.IsNullOrEmpty(text))
                return text;

            //return text.Replace("â€™", "'").Replace("â€¦", "!");
            return text.Replace("’", "'").Replace("…", "...");
        }


        public static string PrepareMensApparelSize(string size)
        {
            string[] allowSizes = new string[]
            {
                "26",
"27",
"28",
"29",
"30",
"31",
"32",
"33",
"34",
"35",
"36",
"37",
"38",
"39",
"40",
"41",
"42",
"43",
"44",
"45",
"46",
"47",
"48",
"49",
"50",
"51",
"52",
"53",
"54",
"55",
"56",
"57",
"58",
"59",
"60",
"61",
"2XLB",
"2XLT",
"34Sx28W",
"36Rx30W",
"36Sx30W",
"38Lx32W",
"38Rx32W",
"38Sx32W",
"3XLB",
"3XLT",
"40Lx34W",
"40Rx34W",
"40Sx34W",
"42Lx36W",
"42Rx36W",
"42Sx36W",
"44Lx38W",
"44Rx38W",
"44Sx38W",
"46Lx40W",
"46Rx40W",
"46Sx40W",
"48Lx42W",
"48Rx42W",
"48Sx42W",
"4XLB",
"4XLT",
"50Lx44W",
"50Rx44W",
"52Lx46W",
"52Rx46W",
"54Lx48W",
"54Rx48W",
"56Lx50W",
"56Rx50W",
"5XLB",
"5XLT",
"6XL",
"6XLB",
"6XLT",
"7XL",
"7XLB",
"7XLT",
"8XL",
"8XLB",
"8XLT",
"Large",
"Large/X-Large",
"LT",
"Medium",
"Medium/Large",
"One Size",
"Small",
"Small/Medium",
"X-Large",
"X-Small",
"XLB",
"XLT",
"XX-Large",
"XX-Small",
"XXX-Large",
"XXX-Small",
"XXXX-Large",
"XXXX-Small",
"XXXXX-Large",
"XXXXX-Small"
            };

            var existSize = allowSizes.FirstOrDefault(s => StringHelper.IsEqualNoCase(s, size));
            if (existSize != null)
                return existSize;

            var digitPart = StringHelper.GetFirstDigitSequences(size);
            if (digitPart.HasValue)
            {
                existSize = allowSizes.FirstOrDefault(s => StringHelper.IsEqualNoCase(s, size));
                if (existSize != null)
                    return existSize + "//" + size;
            }

            if (StringHelper.IsEqualNoCase(size, "XS"))
                return "X-Small";
            if (StringHelper.IsEqualNoCase(size, "S"))
                return "Small";
            if (StringHelper.IsEqualNoCase(size, "S/M"))
                return "Small/Medium";
            if (StringHelper.IsEqualNoCase(size, "M"))
                return "Medium";
            if (StringHelper.IsEqualNoCase(size, "M/L"))
                return "Medium/Large";
            if (StringHelper.IsEqualNoCase(size, "L"))
                return "Large";
            if (StringHelper.IsEqualNoCase(size, "L/XL"))
                return "Large/X-Large";
            if (StringHelper.IsEqualNoCase(size, "XL"))
                return "X-Large";
            if (StringHelper.IsEqualNoCase(size, "2X"))
                return "XX-Large";

            return size;
        }


        public static string PrepareSize(string size)
        {
            string[] allowSizes = new string[]
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
                "10",
                "11",
                "12",
                "13",
                "14",
                "15",
                "16",
                "18",
                "20",
                "30",
                "32",
                "34",
                "36",
                "38",
                "10 husky",
                "10 plus",
                "10 slim",
                "12 husky",
                "12 plus",
                "12 slim",
                "14 husky",
                "14 plus",
                "14 slim",
                "16 husky",
                "16 plus",
                "16 slim",
                "18 husky",
                "18 months",
                "18 plus",
                "18 slim",
                "20 husky",
                "20 plus",
                "24 months",
                "2t",
                "2T-6x",
                "2T-7",
                "2XL",
                "2XS",
                "3t",
                "3XL",
                "3XS",
                "4 slim",
                "4t",
                "4T/4",
                "4XL",
                "4XS",
                "5 slim",
                "5t",
                "5T/5",
                "5XL",
                "5XS",
                "6 slim",
                "6x",
                "7 slim",
                "7-16",
                "7x",
                "8 husky",
                "8 slim",
                "8-20",
                "Baby Boy",
                "Baby Girl",
                "L",
                "large",
                "large plus",
                "M",
                "medium",
                "medium plus",
                "One Size",
                "S",
                "small",
                "x-large",
                "x-small",
                "XL",
                "XS",
                "xx-large",
                "xx-small",
            };
            var existSize = allowSizes.FirstOrDefault(s => StringHelper.IsEqualNoCase(s, size));
            if (existSize != null)
                return existSize;

            var digitPart = StringHelper.GetFirstDigitSequences(size);
            if (digitPart.HasValue)
            {
                existSize = allowSizes.FirstOrDefault(s => StringHelper.IsEqualNoCase(s, size));
                if (existSize != null)
                    return existSize + "//" + size;
            }

            return size;
        }


        public static string PrepareColor(string color)
        {
            string[] allowColors = new string[]
                {
                    "Beige",
                    "Black",
                    "Blue",
                    "Bronze",
                    "Brown",
                    "Clear",
                    "Copper",
                    "Cream",
                    "Gold",
                    "Green",
                    "Grey",
                    "Ivory",
                    "Metallic",
                    "Multi-color",
                    "Off-white",
                    "Orange",
                    "Pink",
                    "Purple",
                    "Red",
                    "Silver",
                    "White",
                    "Yellow"
                };

            foreach (var allowColor in allowColors)
            {
                if (StringHelper.ContainsNoCase(color, allowColor))
                {
                    if (StringHelper.IsEqualNoCase(color, allowColor))
                        return allowColor;
                    return allowColor + " // " + color;
                }
            }
            if (String.IsNullOrEmpty(color))
                return "Multi-color";
            return "Multi-color // " + color;
        }
        
        public static IList<decimal> GetSizes(string shippingSize)
        {
            /*XS – 9x12x0.5
            S -9x12x1
            M-10x15x1
            L-10x15x2
            XL-10x15x3
            */
            if (StringHelper.IsEqualNoCase(shippingSize, "XS"))
            {
                return new decimal[] { 9, 12, 0.5M };
            }
            if (StringHelper.IsEqualNoCase(shippingSize, "S"))
            {
                return new decimal[] { 9, 12, 1 };
            }
            if (StringHelper.IsEqualNoCase(shippingSize, "M") || String.IsNullOrEmpty(shippingSize))
            {
                return new decimal[] { 10, 15, 1 };
            }
            if (StringHelper.IsEqualNoCase(shippingSize, "L"))
            {
                return new decimal[] { 10, 15, 2 };
            }
            if (StringHelper.IsEqualNoCase(shippingSize, "XL"))
            {
                return new decimal[] { 10, 15, 3 };
            }
            return null;
        }


        public static string GetCategories(string gender, string itemStyle)
        {
            if (StringHelper.IsEqualNoCase(itemStyle, "Costumes"))
            {
                if (StringHelper.ContainsNoCase(gender, "girls"))
                {
                    return "Apparel_Girls'_Seasonal_Costumes";
                }
                if (StringHelper.ContainsNoCase(gender, "boys"))
                {
                    return "Apparel_Boys'_Seasonal_Costumes";
                }
                if (StringHelper.ContainsNoCase(gender, "women"))
                {
                    return "Apparel_Women's_Seasonal_Halloween Costumes"; //Apparel_Women's_Seasonal_Costumes
                }
                if (StringHelper.ContainsNoCase(gender, "mens"))
                {
                    return "Apparel_Men's_Seasonal_Halloween Costumes_Bodysets";
                }
                return "Apparel_Women's_Seasonal_Halloween Costumes";
            }


            if (StringHelper.IsEqualNoCase(itemStyle, "Underwear"))
            {
                if (StringHelper.ContainsNoCase(gender, "boys"))
                {
                    return "Apparel_Boys'_Underwear";
                }
                if (StringHelper.ContainsNoCase(gender, "girls"))
                {
                    return "Apparel_Girls'_Underwear";
                }
                if (StringHelper.ContainsNoCase(gender, "women"))
                {
                    return "Apparel_Women's_Intimates_Slips";
                }
                if (StringHelper.ContainsNoCase(gender, "men"))
                {
                    return "Apparel_Men's_Underwear_Collections & Sets";
                }
            }

            if (StringHelper.IsEqualNoCase(itemStyle, "Footed"))
            {
                if (StringHelper.ContainsNoCase(gender, "boys"))
                {
                    return "Apparel_Boys'_Pajamas"; //Apparel_Boys'_Pajamas //Apparel_Boys'_Sleepwear_Gowns
                }
                if (StringHelper.ContainsNoCase(gender, "girls"))
                {
                    return "Apparel_Girls'_Pajamas"; //Apparel_Girls'_Pajamas //Apparel_Girls'_Sleepwear_Gowns
                }
                if (StringHelper.ContainsNoCase(gender, "women"))
                {
                    return "Apparel_Men's_Sleep & Lounge_Sleep Sets";
                }
                if (StringHelper.ContainsNoCase(gender, "men"))
                {
                    return "Apparel_Men's_Sleep & Lounge_Sleep Sets";
                }
            }

            if (StringHelper.IsEqualNoCase(itemStyle, "Nightgown") 
                || StringHelper.ContainsNoCase(itemStyle, "pajama")
                || StringHelper.ContainsNoCase(itemStyle, "2 Piece Set")
                || StringHelper.ContainsNoCase(itemStyle, "Onesie"))
            {
                if (StringHelper.ContainsNoCase(gender, "boys"))
                {
                    return "Apparel_Boys'_Pajamas"; //Apparel_Boys'_Pajamas //Apparel_Boys'_Sleepwear_Gowns
                }
                if (StringHelper.ContainsNoCase(gender, "girls"))
                {
                    return "Apparel_Girls'_Pajamas"; //Apparel_Girls'_Pajamas //Apparel_Girls'_Sleepwear_Gowns
                }
                if (StringHelper.ContainsNoCase(gender, "women"))
                {
                    return "Apparel_Women's_Sleep & Lounge_Sleep Sets";
                }
                if (StringHelper.ContainsNoCase(gender, "men"))
                {
                    return "Apparel_Men's_Sleep & Lounge_Sleep Sets";
                }
                return "Apparel_Women's_Sleep & Lounge_Sleep Sets";
            }

            if (StringHelper.IsEqualNoCase(itemStyle, "Robe"))
            {
                if (StringHelper.ContainsNoCase(gender, "boys"))
                {
                    return "Apparel_Boys'_Bath_Robes";
                }
                if (StringHelper.ContainsNoCase(gender, "girls"))
                {
                    return "Apparel_Girls'_Bath_Robes";
                }
                if (StringHelper.ContainsNoCase(gender, "women"))
                {
                    return "Apparel_Women's_Sleep & Lounge_Robes";
                }
                if (StringHelper.ContainsNoCase(gender, "men"))
                {
                    return "Apparel_Men's_Sleep & Lounge_Robes";
                }
            }

            if (StringHelper.IsEqualNoCase(itemStyle, "Shirt"))
            {
                if (StringHelper.ContainsNoCase(gender, "men"))
                {
                    return "Apparel_Men's_Sleep & Lounge_Sleep Sets";
                }
            }

            if (StringHelper.IsEqualNoCase(itemStyle, "Shorts"))
            {
                if (StringHelper.ContainsNoCase(gender, "boys"))
                {
                    return "Apparel_Boys'_Bottoms & Overalls_Shorts";
                }
                if (StringHelper.ContainsNoCase(gender, "girls"))
                {
                    return "Apparel_Girls'_Bottoms & Overalls_Shorts";
                }
                if (StringHelper.ContainsNoCase(gender, "women"))
                {
                    return "Apparel_Women's_Sleep & Lounge_Sleep Sets";// "Apparel_Women's_Bottoms_Shorts";
                }
                if (StringHelper.ContainsNoCase(gender, "men"))
                {
                    return "Apparel_Men's_Sleep & Lounge_Sleep Sets";// "Apparel_Men's_Shorts_Denim";
                }
            }

            if (StringHelper.IsEqualNoCase(itemStyle, "Pants") || StringHelper.IsEqualNoCase(itemStyle, "Sleepwear Pants"))
            {
                if (StringHelper.ContainsNoCase(gender, "boys"))
                {
                    return "Apparel_Boys'_Bottoms & Overalls_Pants";
                }
                if (StringHelper.ContainsNoCase(gender, "girls"))
                {
                    return "Apparel_Girls'_Bottoms & Overalls_Pants";
                }
                if (StringHelper.ContainsNoCase(gender, "women"))
                {
                    return "Apparel_Women's_Sleep & Lounge_Sleep Bottoms"; //Apparel_Women's_Bottoms_Pants
                }
                if (StringHelper.ContainsNoCase(gender, "men"))
                {
                    return "Apparel_Men's_Sleep & Lounge_Sleep Bottoms";
                }
            }

            return null;
        }
    }
}
