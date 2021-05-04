using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls.WebParts;
using Amazon.Utils;

namespace Amazon.Common.Helpers
{
    public class  StringHelper
    {
        public static string[] ToArray(params string[] values)
        {
            return values.ToList().Where(v => !String.IsNullOrEmpty(v)).ToArray();
        }
        
        public static string RemoveAllNonASCII(string text)
        {
            if (String.IsNullOrEmpty(text))
                return text;
            return Regex.Replace(text, @"[^\u0000-\u007F]+", string.Empty);
        }

        public static List<string> GetTextsBetweenTags(string tag, string text)
        {
            var regex = new Regex(String.Format(@"<\s*{0}[^>]*>(.*?)<\s*/\s*{0}>", tag), RegexOptions.Multiline);
            var matches = regex.Matches(text);
            var results = new List<string>();
            
            foreach (Group match in matches)
            {
                results.Add(match.Value);
            }

            return results;
        }

        public static string PrepareToLog(string text)
        {
            if (String.IsNullOrEmpty(text))
                return text;
            return text.Replace("<br/>", "\r\n");
        }

        public static string NullIfEmpty(string text)
        {
            if (String.IsNullOrEmpty(text))
                return null;
            return text;
        }

        public static bool IsDigit(string text, char[] additionalChars)
        {
            if (String.IsNullOrEmpty(text))
                return false;

            return text.All(ch => Char.IsDigit(ch) || additionalChars.Contains(ch));
        }

        public static string AddSpacesBeforeUpperCaseLetters(string text)
        {
            if (String.IsNullOrEmpty(text))
                return text;

            var result = String.Empty;
            for (var i = 0; i < text.Length; i++)
            {
                if (Char.IsUpper(text[i])
                    && i - 1 >= 0
                    && text[i - 1] != ' ')
                    result += " " + text[i];
                else
                    result += text[i];
            }

            return result;
        }

        public static string ToYesNo(bool? val)
        {
            if (val == true)
                return "Yes";
            return "No";
        }

        public static string IfEmpty(string val, string defVal)
        {
            if (String.IsNullOrEmpty(val))
                return defVal;
            return val;
        }

        public static string WrapCData(string text)
        {
            if (string.IsNullOrEmpty(text))
                return "";
            return "<![CDATA[" + text + "]]>";
        }

        public static string ConvertToWordNumber(int qty)
        {
            switch (qty)
            {
                case 0:
                    return "zero";
                case 1:
                    return "one";
                case 2:
                    return "two";
                case 3:
                    return "three";
                case 4:
                    return "four";
                case 5:
                    return "five";
                case 6:
                    return "six";
                case 7:
                    return "seven";
                case 8:
                    return "eight";
                case 9:
                    return "nine";
                case 10:
                    return "ten";
                default:
                    return qty + " x";
            }
        }

        public static string MakeEachWordFirstLetterUpper(string text)
        {
            if (String.IsNullOrEmpty(text))
                return text;

            var parts = text.Split(" -.,/_+()".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < parts.Length; i++)
            {
                parts[i] = MakeFirstLetterUpper(parts[i]);
            }

            return String.Join(" ", parts);
        }

        public static string MakeFirstLetterUpper(string text, bool toLowerOther = true)
        {
            if (String.IsNullOrEmpty(text))
                return text;

            if (text.Length > 0)
            {
                return text.Substring(0, 1).ToUpper() + (toLowerOther ? text.Substring(1).ToLower() : text.Substring(1));
            }
            return text;
        }

        public static string TrimTags(string html)
        {
            if (String.IsNullOrEmpty(html))
                return html;

            string woTagSections = html;
            var sections = new string[] { "head", "script", "style" };
            foreach (var section in sections)
            {
                woTagSections = RemoveAllBetweenTags(woTagSections, section);
            }

            string noHTML = woTagSections;
            if (!String.IsNullOrEmpty(woTagSections))
                noHTML = (Regex.Replace(woTagSections, @"<[^>]+>|&nbsp;", "") ?? "").Trim();
            if (!String.IsNullOrEmpty(noHTML))
                noHTML = noHTML.Replace("&ldquo;", "\"").Replace("&rdquo;", "\"");

            if (!String.IsNullOrEmpty(noHTML))
            {
                string noHTMLNormalised = Regex.Replace(noHTML, @"\s{2,}", " ");
                return noHTMLNormalised;
            }
            else
            {
                return noHTML;
            }
        }

        public static string RemoveAllBetweenTags(string text, string tagName)
        {
            if (String.IsNullOrEmpty(text))
                return null;

            var startTag = "<" + tagName;
            var endTag = "</" + tagName + ">";
            var startTagIndex = 0;
            var endTagIndex = 0;
            do
            {
                startTagIndex = text.IndexOf(startTag, startTagIndex);
                if (startTagIndex >= 0)
                {
                    endTagIndex = text.IndexOf(endTag, startTagIndex);
                    if (endTagIndex >= 0)
                    {
                        text = text.Substring(0, startTagIndex) + text.Substring(endTagIndex + endTag.Length);
                    }
                }
            } while (startTagIndex >= 0 && endTagIndex >= 0);

            return text;
        }

        public static int? FindText(string text, string searchText)
        {
            if (String.IsNullOrEmpty(text))
                return null;
            if (String.IsNullOrEmpty(searchText))
                return null;
            var result = text.IndexOf(searchText, StringComparison.InvariantCultureIgnoreCase);
            if (result >= 0)
                return result;

            return null;
        }

        public static int? FindLastIndexOfTheSentenenceEnd(string text, int beforeIndex)
        {
            if (String.IsNullOrEmpty(text))
                return null;
            if (text.Length <= beforeIndex)
                return null;
            var index = text.LastIndexOfAny(".!?".ToCharArray(), beforeIndex);
            if (index > 0)
                return index + 1;
            return null;
        }

        public static int SuquenceSearch(string text, string[] keywords)
        {
            if (String.IsNullOrEmpty(text))
                return -1;

            var startIndex = 0;
            foreach (var key in keywords)
            {
                startIndex = text.IndexOf(key, startIndex, StringComparison.InvariantCultureIgnoreCase);
                if (startIndex >= 0)
                    startIndex += key.Length;
                else
                    return -1;
            }

            return startIndex;
        }

        public static string RemoveAfter(string text, string keyword)
        {
            return RemoveAfter(text, new string[] { keyword });
        }

        public static string RemoveAfter(string text, string[] keywords)
        {
            if (String.IsNullOrEmpty(text))
                return text;

            var minIndex = -1;
            foreach (var keyword in keywords)
            {
                var index = text.IndexOf(keyword, StringComparison.InvariantCultureIgnoreCase);
                if (index > 0)
                    minIndex = index;
            }
            if (minIndex > 0)
                return text.Substring(0, minIndex);
            return text;
        }

        public static string Substring(string text, int length)
        {
            if (String.IsNullOrEmpty(text))
                return text;
            if (text.Length <= length)
                return text;
            return text.Substring(0, length);
        }

        public static string SubstringEndWithWholeWord(string text, int length)
        {
            var chars = new char[] { ',', '.', ' ', '!', '?', '/', '-' };
            if (String.IsNullOrEmpty(text))
                return text;
            if (text.Length <= length)
                return text;

            var index = text.LastIndexOfAny(chars, 80);
            if (index >= 0)
                return text.Substring(0, index);

            return text.Substring(0, length);
        }

        public static string Substring(string text, int startIndex, int length, string trailingText = "")
        {
            if (String.IsNullOrEmpty(text))
                return text;

            if (text.Length <= startIndex)
                return "";

            if (text.Length <= startIndex + length)
                return text;
            return text.Substring(startIndex, length) + trailingText;
        }

        public static bool IsMatchWithAny(string source, IList<string> candidateList)
        {
            if (String.IsNullOrEmpty(source))
                return false;

            if (candidateList == null)
                return false;

            foreach (var candidate in candidateList)
            {
                if (String.Compare(source, candidate, StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool ContrainOneOfKeywords(string source, IList<string> strings)
        {
            if (String.IsNullOrEmpty(source))
                return false;

            if (strings == null)
                return false;

            foreach (var str in strings)
            {
                if (!String.IsNullOrEmpty(str))
                {
                    if (source.IndexOf(str, StringComparison.InvariantCultureIgnoreCase) >= 0)
                        return true;
                }
            }

            return false;
        }

        public static bool GetInOneOfStrings(string source, IList<string> strings)
        {
            if (String.IsNullOrEmpty(source))
                return false;

            if (strings == null)
                return false;

            foreach (var str in strings)
            {
                if (!String.IsNullOrEmpty(str))
                {
                    if (str.IndexOf(source, StringComparison.InvariantCultureIgnoreCase) >= 0)
                        return true;
                }
            }

            return false;
        }

        public static bool EqualWithOneOfStrings(string source, IList<string> strings)
        {
            if (String.IsNullOrEmpty(source))
                return false;

            if (strings == null)
                return false;

            foreach (var str in strings)
            {
                if (!String.IsNullOrEmpty(str))
                {
                    if (string.Compare(str, source, StringComparison.InvariantCultureIgnoreCase) == 0)
                        return true;
                }
            }

            return false;
        }

        public static string ReplaceString(string str, string oldValue, string newValue, StringComparison comparison)
        {
            StringBuilder sb = new StringBuilder();

            int previousIndex = 0;
            int index = str.IndexOf(oldValue, comparison);
            while (index != -1)
            {
                sb.Append(str.Substring(previousIndex, index - previousIndex));
                sb.Append(newValue);
                index += oldValue.Length;

                previousIndex = index;
                index = str.IndexOf(oldValue, index, comparison);
            }
            sb.Append(str.Substring(previousIndex));

            return sb.ToString();
        }

        public static string Replaces(string source, IDictionary<string, string> replaces)
        {
            if (String.IsNullOrEmpty(source))
                return String.Empty;

            if (replaces == null)
                return source;

            foreach (var replace in replaces)
                source = source.Replace(replace.Key, replace.Value);

            return source;
        }

        public static int? ToInt(string str)
        {
            int res = 0;
            if (Int32.TryParse(str, out res))
                return res;
            return null;
        }

        public static string JoinTwo(string separator, string str1, string str2)
        {
            if (String.IsNullOrEmpty(str1))
                return str2;
            if (String.IsNullOrEmpty(str2))
                return str1;
            return str1 + separator + str2;
        }

        public static string Join(string separator, params string[] strings)
        {
            var result = "";
            if (strings == null)
                return result;

            foreach (var str in strings)
            {
                if (!String.IsNullOrEmpty(str))
                {
                    result += (!String.IsNullOrEmpty(result) ? separator : "") + str;
                }
            }

            return result;
        }

        public static string GetFirstNotEmpty(params string[] args)
        {
            foreach (var str in args)
                if (!String.IsNullOrEmpty(str))
                    return str;
            return null;
        }

        public static string[] Split(string value, string separator)
        {
            if (String.IsNullOrEmpty(value))
                return new string[] {};
            return value.Split(new string[] { separator }, StringSplitOptions.RemoveEmptyEntries);
        }

        public static string ToUpper(string text)
        {
            if (String.IsNullOrEmpty(text))
                return text;
            return text.ToUpper();
        }

        public static T TryGetEnum<T>(string text, T defValue) where T : struct
        {
            T result;
            if (Enum.TryParse(text, out result))
                return (T) result;
            return defValue;
        }

        public static int? TryGetInt(string text)
        {
            if (String.IsNullOrEmpty(text))
                return null;

            int result;
            if (Int32.TryParse(text, out result))
                return result;

            return null;
        }

        public static decimal? TryGetDecimal(string text)
        {
            if (string.IsNullOrEmpty(text))
                return null;

            decimal result;
            if (decimal.TryParse(text, out result))
                return result;

            return null;
        }

        public static long? TryGetLong(string text)
        {
            if (String.IsNullOrEmpty(text))
                return null;

            long result;
            if (long.TryParse(text, out result))
                return result;

            return null;
        }

        public static string GetFirstPartOrEmpty(string multistring, string separators)
        {
            if (String.IsNullOrEmpty(multistring))
                return null;
            var parts = multistring.Split(separators.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 0)
                return parts[0];
            return null;
        }

        public static string GetTextBetween(string text, string startText, string[] endTexts)
        {
            if (string.IsNullOrEmpty(text))
                return null;
            var startIndex = StringHelper.FindText(text, startText);
            if (startIndex >= 0)
            {
                var endIndex = StringHelper.FindIndexOfAny(text, endTexts, startIndex.Value);
                if (endIndex >= 0)
                {
                    return text.Substring(startIndex.Value + startText.Length, endIndex.Value - startIndex.Value);
                }
            }
            return null;
        }

        public static int? FindIndexOfAny(string text, string[] findTexts, int? startIndex)
        {
            if (string.IsNullOrEmpty(text))
                return null;
            if (findTexts == null || !findTexts.Any())
                return null;

            int? resultIndex = null;
            foreach (var findText in findTexts)
            {
                if (!String.IsNullOrEmpty(findText))
                {
                    var index = text.IndexOf(findText, startIndex ?? 0, StringComparison.InvariantCultureIgnoreCase);
                    if (index >= 0
                        || (resultIndex.HasValue && index < resultIndex.Value))
                        resultIndex = index;
                }
            }
            return resultIndex;            
        }

        public static string RemoveWhitespace(string text)
        {
            if (String.IsNullOrEmpty(text))
                return text;
            return Regex.Replace(text, @"\s+", "");
        }

        public static string RemoveStrings(string text, IList<string> strs)
        {
            if (strs == null)
                return text;

            if (String.IsNullOrEmpty(text))
                return text;

            foreach (var str in strs)
            {
                if (String.IsNullOrEmpty(str))
                    continue;
                text = text.Replace(str, "");
            }
            return text;
        }

        public static string ReplaceMultipleWhitespaceWithOne(string text)
        {
            return Regex.Replace(text, @"\s+", " ");
        }

        public static string Trim(string text, char[] symbols)
        {
            if (String.IsNullOrEmpty(text))
                return text;
            return text.Trim(symbols);
        }

        public static string TrimWhitespace(string text)
        {
            var result = Trim(text, new[] {'\r', '\n', '\t', ' ',
                '\u0160' //Unicode of nonbreaking excel space
            });
            if (result != null)
                return result.Trim();
            return result;
        }

        public static string GetAllDigitSequences(string text)
        {
            try
            {
                if (String.IsNullOrEmpty(text))
                    return text;

                //Find first digit sequences
                String digits = String.Empty;
                for (int i = 0; i < text.Length; i++)
                {
                    if (Char.IsDigit(text[i]))
                        digits += text[i];
                }
                return digits;
            }
            catch { }
            return null;
        }

        public static int? GetFirstDigitSequences(string text)
        {
            try
            {
                if (String.IsNullOrEmpty(text))
                    return null;

                //Find first digit sequences
                String digit = String.Empty;
                for (int i = 0; i < text.Length; i++)
                {
                    if (Char.IsDigit(text[i]))
                        digit += text[i];
                    else if (digit.Length > 0)
                        break;
                }

                int value = 0;
                if (digit.Length > 0)
                    value = GeneralUtils.TryGetInt(digit, 0);
                
                return value;
            }
            catch { }
            return null;
        }

        public static byte[] FromBase64(string text)
        {
            return Convert.FromBase64String(text);
        }

        public static string ToBase64(byte[] bytes)
        {
            return Convert.ToBase64String(bytes);
        }

        public static string ToBase64(string text)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(text));
        }

        public static bool IsEqualNoCase(string text1, string text2)
        {
            return String.Compare(text1, text2, StringComparison.InvariantCultureIgnoreCase) == 0;
        }

        public static bool IsEqualNoCaseTrim(string text1, string text2)
        {
            return String.Compare((text1 ?? "").Trim(), (text2 ?? "").Trim(), StringComparison.InvariantCultureIgnoreCase) == 0;

        }

        public static bool IsEqualWithCase(string text1, string text2)
        {
            return String.Compare(text1, text2, StringComparison.InvariantCulture) == 0;
        }

        public static bool ContainsNoCase(string text1, string text2)
        {
            if (String.IsNullOrEmpty(text1)
                || String.IsNullOrEmpty(text2))
                return false;

            return text1.IndexOf(text2, StringComparison.InvariantCultureIgnoreCase) >= 0;
        }

        public static bool ContainsWithCase(string text1, string text2)
        {
            if (String.IsNullOrEmpty(text1)
                || String.IsNullOrEmpty(text2))
                return false;

            return text1.IndexOf(text2, StringComparison.InvariantCulture) >= 0;
        }

        public static string RemoveAllNonAlphanumericAndPunctuation(string text)
        {
            if (String.IsNullOrEmpty(text))
                return text;
            Regex rgx = new Regex("[^a-zA-Z0-9 -+,.!?]");
            return rgx.Replace(text, "");
        }

        public static string KeepOnlyAlphanumeric(string text, char[] includeChars = null, string replaceChar = null)
        {
            if (String.IsNullOrEmpty(text))
                return text;
            
            Regex rgx = new Regex(String.Format("[^a-zA-Z0-9{0}]", includeChars != null ? Regex.Escape(new String(includeChars)).Replace("-", "\\-") : String.Empty));
            return rgx.Replace(text, (replaceChar ??  ""));
        }
    }
}
