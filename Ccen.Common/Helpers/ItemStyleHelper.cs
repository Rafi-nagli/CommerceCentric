using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core.Models;

namespace Amazon.Common.Helpers
{
    public class ItemStyleHelper
    {
        private static bool IsSaras(string title)
        {
            var name = title ?? "";
            if (name.ToLower().Contains("saras") || name.ToLower().Contains("sara's"))
                return true;
            return false;
        }

        public static ItemStyleType GetFromItemStyleOrTitle(string itemStyle, string title)
        {
            if (String.IsNullOrEmpty(itemStyle))
                return GetFromTitle(title);

            if (itemStyle.ToLower().Contains("nightgown"))
            {
                if (IsSaras(title))
                    return ItemStyleType.NightgownSaras;
                return ItemStyleType.Nightgown;
            }

            if (itemStyle.ToLower().Contains("robe"))
            {
                return ItemStyleType.Robe;
            }

            if ((StringHelper.ContainsNoCase(title, "costume") && String.IsNullOrEmpty(itemStyle))
                || StringHelper.ContainsNoCase(itemStyle, "costumes"))
            {
                return ItemStyleType.Costumes;
            }

            return ItemStyleType.None;
        }

        public static ItemStyleType GetFromTitle(string name)
        {
            if (String.IsNullOrEmpty(name))
                return ItemStyleType.None;

            if (name.ToLower().Contains("nightgown") || name.ToLower().Contains("nighty"))
            {
                if (IsSaras(name))
                    return ItemStyleType.NightgownSaras;
                return ItemStyleType.Nightgown;
            }

            if (name.ToLower().Contains("robe"))
                return ItemStyleType.Robe;

            if (StringHelper.ContainsNoCase(name, "costume"))
                return ItemStyleType.Costumes;

            return ItemStyleType.None;
        }
    }
}
