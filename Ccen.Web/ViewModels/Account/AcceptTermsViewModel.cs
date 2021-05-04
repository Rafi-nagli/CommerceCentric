using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Amazon.Web.ViewModels.Account
{
    public class AcceptTermsViewModel
    {
        private IDbFactory _dbFactory;
        private ILogService _log;
        private ITime _time;

        public AcceptTermsViewModel(IDbFactory dbFactory,
            ILogService log,
            ITime time)
        {
            _dbFactory = dbFactory;
            _log = log;
            _time = time;
        }
        
        public void Accept(long userId, HttpRequestBase request)
        {
            String ip = null;
            try
            {
                ip = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                if (string.IsNullOrEmpty(ip))
                {
                    ip = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
                }
            }
            catch (Exception ex)
            {

            }

            var details = "DateTime (EST): " + _time.GetAppNowTime()
                + "\r\n UserHostAddress: " + request.UserHostAddress 
                + "\r\n IPAddress: " + ip
                + "\r\n Browser: " + request.Browser?.Browser + ", " + request.Browser?.Version
                + "\r\n UrlReferrer: " + request.UrlReferrer
                + "\r\n Url: " + request.Url;

            using (var db = _dbFactory.GetRWDb())
            {
                var user = db.Users.Get(userId);
                user.IsAcceptedTerms = true;
                user.AcceptTermsDetails = details;
                db.Commit();

                AccessManager.User.IsAcceptedTerms = true;
            }
        }
    }
}