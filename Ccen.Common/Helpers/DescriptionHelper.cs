using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.Common.Helpers;

namespace Amazon.Model.Implementation
{
    public class DescriptionHelper
    {
        public static string PrepareForPublishDescription(string description)
        {
            if (string.IsNullOrEmpty(description))
                return "";

            var hasTable = description.IndexOf("<table") >= 0;
            if (hasTable) //NOTE: BNW description/spec table
            {
                var beginTableIndex = description.IndexOf("<table");
                var endTableIndex = description.IndexOf("/table>");
                if (beginTableIndex >= 0 && endTableIndex >= 0)
                {
                    var text = description.Remove(beginTableIndex, endTableIndex + 7 - beginTableIndex);
                    var noHtml = StringHelper.TrimTags(text);
                    return noHtml;
                }
                return "";
            }

            var endIndex = description.LastIndexOf("</div>");
            var beginIndex = description.LastIndexOf("<div>") + 5;
            if (endIndex >= 0 && beginIndex < endIndex)
                return description.Substring(beginIndex, endIndex - beginIndex);

            if (endIndex == -1) //No html
                return description;
            return "";
        }
    }
}
