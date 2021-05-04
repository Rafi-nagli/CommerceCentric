using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Amazon.Web.Models
{
    public static class ModelStateExtensions
    {
        public static IList<string> GetErrors(this ModelStateDictionary sender)
        {
            var results = new List<string>();
            var errors = sender.Values.Where(s => s.Errors != null && s.Errors.Any()).Select(s => s.Errors).ToList();
            foreach (var error in errors)
            {
                results.AddRange(error.Select(e => e.ErrorMessage).ToList());
            }
            return results;
        }
    }
}