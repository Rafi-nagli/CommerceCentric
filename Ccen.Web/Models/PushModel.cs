using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Amazon.Web.Models
{
    public class PushModel
    {
        public string Email { get; set; }
        public string RegistrationId { get; set; }

        public override string ToString()
        {
            return "Email=" + Email + ", RegistrationId=" + RegistrationId;
        }
    }
}