﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Helpers
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ToStringIgnoreAttribute : Attribute
    {
        public ToStringIgnoreAttribute()
        {
            
        }
    }
}
