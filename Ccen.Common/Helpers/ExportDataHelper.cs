using Amazon.Common.ExcelExport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Common.Helpers
{
    public class ExportDataHelper
    {

        public static string BuildSearchTerms(string itemStyle, string material, string sleeve)
        {
            var searchTerms = "";
            if ((itemStyle ?? "").ToLower().Contains("pajama"))
                searchTerms = String.Format("sleepwear, pj, jummie, new, {0}", (sleeve ?? "").ToLower());
            if ((itemStyle ?? "").ToLower().Contains("nightgown"))
                searchTerms = String.Format("night, gown, night-gown, sleepwear, pj, jummie, new, {0}, dress up", (sleeve ?? "").ToLower());

            searchTerms += StringHelper.JoinTwo(", ", searchTerms, "Gift, present, 2017, cozy");
            if ((material ?? "").ToLower().Contains("fleece"))
                searchTerms += ", fleece, microfleece, warm, winter spring";

            return searchTerms;
        }

        public static IList<string> BuildKeyFeatures(string mainLicense, 
            string subLicense, 
            string material,
            string gender)
        {            
            var features = new List<string>();
            var isKids = StringHelper.EqualWithOneOfStrings(gender, new List<string>() { "boys", "girls" });

            if (mainLicense != "Generic")
                features.Add("Authentic " + mainLicense + " product with reliable quality and durability");
            features.Add("Featuring " + (String.IsNullOrEmpty(subLicense) ? mainLicense : subLicense));
            features.Add((material ?? "").ToLower().Contains("cotton") ? "100% Cotton" : (isKids ? "Flame resistant" : ""));
            features.Add("Machine Wash, Easy Care");

            return features;
        }
    }
}
