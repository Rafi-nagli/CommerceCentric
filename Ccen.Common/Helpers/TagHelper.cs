using Amazon.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ccen.Common.Helpers
{
    public class TagsHelper
    {
        public static string AddTags(string sourceTags, IList<string> tagsToAdd)
        {
            var existTags = (sourceTags ?? "").Split(",".ToCharArray());

            var resultTags = existTags.ToList();
            foreach (var tag in tagsToAdd)
            {
                if (!resultTags.Any(k => StringHelper.ContainsNoCase(tag, k)))
                resultTags.Add(tag);
            }

            var result = String.Join(",", resultTags);
            return result;
        }

        public static string RemoveTags(string sourceTags, IList<string> tagsToDelete)
        {
            var existTags = sourceTags.Split(",".ToCharArray());

            var resultTags = new List<string>();
            foreach (var tag in existTags)
            {
                if (!tagsToDelete.Any(k => StringHelper.ContainsNoCase(tag, k)))
                    resultTags.Add(tag);
            }

            var result = String.Join(",", resultTags);
            return result;
        }
    }
}
