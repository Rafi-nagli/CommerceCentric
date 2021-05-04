using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Amazon.Web.ModelBinder
{
    public class DateTimeModelBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext,
            ModelBindingContext bindingContext)
        {
            ValueProviderResult valueResult = bindingContext.ValueProvider
                .GetValue(bindingContext.ModelName);
            ModelState modelState = new ModelState { Value = valueResult };
            object actualValue = null;
            if (valueResult != null)
            {
                try
                {
                    if (!String.IsNullOrEmpty(valueResult.AttemptedValue))
                    {
                        actualValue = TryParseAsJavaScriptDate(valueResult.AttemptedValue);
                        if (actualValue == null)
                        {
                            actualValue = TryParseAsDateTime(valueResult.AttemptedValue);
                        }
                    }
                }
                catch (FormatException e)
                {
                    modelState.Errors.Add(e);
                }
            }

            bindingContext.ModelState.Add(bindingContext.ModelName, modelState);
            return actualValue;
        }

        private DateTime? TryParseAsJavaScriptDate(string value)
        {
            if (value.StartsWith("/Date("))
            {
                try
                {
                    DateTime date = new DateTime(1970, 01, 01, 0, 0, 0, DateTimeKind.Utc).ToUniversalTime();
                    string attemptedValue = value.Replace("/Date(", "").Replace(")/", "");
                    double milliSecondsOffset = Convert.ToDouble(attemptedValue);
                    DateTime result = date.AddMilliseconds(milliSecondsOffset);                    
                    result = result.ToUniversalTime();
                    return result;
                }
                catch
                {
                }
            }
            return null;
        }

        private DateTime? TryParseAsDateTime(string value)
        {
            DateTime? result = null;
            try
            {
                result = Convert.ToDateTime(value, new CultureInfo("en-US", false));

            }
            catch (FormatException ex)
            {
                //Additional checking following formats

                //Thu Apr 23 2015 07:40:57 GMT-0400 (Eastern Daylight Time)
                var formats = new[] { "ddd, d MMM yyyy HH:mm:ss UTC", "ddd MMM d yyyy HH:mm:ss \"GMT\"zzz" };
                var valueWithoutTimezone = RemoveFullTimezoneName(value);

                DateTime resultDate;
                if (DateTime.TryParseExact(valueWithoutTimezone,
                    formats,
                    new CultureInfo("en-US", false),
                    DateTimeStyles.AssumeUniversal,
                    out resultDate))
                {
                    result = resultDate;
                }
                else
                {
                    throw ex; //otherwise rethrow exception
                }
            }

            return result;
        }

        private string RemoveFullTimezoneName(string dateString)
        {
            var parts = dateString.Split(' ');
            return String.Join(" ", parts.Take(6));
        }
    }
}