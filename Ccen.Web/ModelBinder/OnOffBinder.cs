using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Amazon.Web.Binder
{
    public class OnOffBinder : DefaultModelBinder
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (value != null)
            {
                if (value.AttemptedValue == "on")
                {
                    return true;
                }
                else if (value.AttemptedValue == "off")
                {
                    return false;
                }
            }
            return base.BindModel(controllerContext, bindingContext);
        }
    }
}