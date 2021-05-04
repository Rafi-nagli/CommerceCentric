using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Amazon.Core.Contracts;

namespace Amazon.ReportParser.LineParser
{
    public class LineParserHelper
    {
        private static string _defaultFormat = "yyyy-MM-dd HH:mm:ss zzz";

        public static decimal? GetPrice(string val)
        {
            if (!string.IsNullOrEmpty(val))
            {
                decimal result;
                if (decimal.TryParse(val, NumberStyles.Number, CultureInfo.InvariantCulture, out result))
                {
                    return result;
                }
                else
                {
                    throw new FormatException("Decimal value format: " + val);
                }
            }
            return null;
        }

        public static decimal? TryGetPrice(string val)
        {
            if (!string.IsNullOrEmpty(val))
            {
                decimal result;
                if (decimal.TryParse(val, NumberStyles.Number, CultureInfo.InvariantCulture, out result))
                {
                    return result;
                }
            }
            return null;
        }

        public static int? GetAmount(string val)
        {
            if (!string.IsNullOrEmpty(val))
            {
                return int.Parse(val, CultureInfo.InvariantCulture.NumberFormat);
            }
            return null;
        }

        static public bool GetBoolVal(string val, string truePattern = "y", string falsePattern = "n", bool defVal = true)
        {
            if (string.IsNullOrWhiteSpace(val))
            {
                return false;
            }
            val = val.ToLower();
            if (val == truePattern)
            {
                return true;
            }
            if (val == falsePattern)
            {
                return false;
            }
            return defVal;
        }

        static public DateTime? GetDateVal(ILogService log,
            string val, 
            int column, 
            string format = null, 
            IFormatProvider prov = null, 
            DateTimeStyles dtStyles = DateTimeStyles.None)
        {
            try
            {
                if (String.IsNullOrEmpty(val))
                {
                    return null;
                }

                DateTime dtRes;
                if (string.IsNullOrEmpty(format))
                {
                    return ConvertDateTime(val);

                }
                if (DateTime.TryParseExact(val, format, prov, dtStyles, out dtRes))
                {
                    return dtRes;
                }

                //ex: 2016-11-25 18:03:17 PST	, 2016-07-06 09:37:29 PDT
                if (DateTime.TryParseExact(val.Replace("PDT", "-07:00").Replace("PST", "-8:00"), _defaultFormat, prov, dtStyles, out dtRes))
                {
                    return dtRes;
                }
                log.Error(String.Format("Parser: can't parse Date string; columnName={0}, value={1}, format={2}", column, val, format));
            }
            catch (Exception ex)
            {
                log.Error(String.Format("Parser: can't parse Date string; columnName={0}, value={1}", column, val), ex);
            }

            return null;
        }

        public static DateTime? ConvertDateTime(string payPalDateTime)
        {
            int offset;
            if (payPalDateTime.EndsWith(" PDT"))
            {
                offset = 7;
            }
            else if (payPalDateTime.EndsWith(" PST"))
            {
                offset = 8;
            }
            else
            {
                return null;
            }

            //9/13/16 3:09:01 AM
            payPalDateTime = payPalDateTime.Substring(0, payPalDateTime.Length - 4);

            string[] dateFormats = { "yyyy-MM-dd HH:mm:ss",
                "yyyy-MM-dd hh:mm:ss tt",
                "MM/d/yy hh:mm:ss tt",
                "M/d/yy hh:mm:ss tt",
                "M/d/yy h:mm:ss tt",
                "MM/d/yy h:mm:ss tt",
                "HH:mm:ss MMM dd, yyyy",
                "HH:mm:ss MMM. dd, yyyy" };

            var res = DateTime.ParseExact(payPalDateTime, dateFormats, new CultureInfo("en-US"), DateTimeStyles.None);

            return res.AddHours(offset);
        }
    }
}
