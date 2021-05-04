using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Web.Mvc;
using Amazon.Core.Entities;
using Amazon.Core.Models;
using Amazon.Core.Models.Calls;
using Amazon.Model.Implementation;
using Amazon.Web.Models;
using Amazon.Web.Models.SearchFilters;
using Amazon.Web.ViewModels;
using Amazon.Web.ViewModels.Emails;
using Amazon.Web.ViewModels.Messages;
using Kendo.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

namespace Amazon.Web.Controllers
{
    [Authorize(Roles = AccessManager.AllBaseRole)]
    public partial class FeedbackController : BaseController
    {
        public override string TAG
        {
            get { return "FeedbackController."; }
        }

        public virtual ActionResult Index()
        {
            LogI("Index");

            return View(FeedbackFilterViewModel.Empty);
        }

        public virtual ActionResult GetAll([DataSourceRequest]DataSourceRequest request,
            DateTime? dateFrom,
            DateTime? dateTo,
            string buyerName,
            string orderNumber,
            string feedbackStatus)
        {
            LogI("GetAll");

            var filter = new FeedbackFilterViewModel()
            {
                DateFrom = dateFrom,
                DateTo = dateTo,
                BuyerName = buyerName,
                OrderNumber = orderNumber,
                FeedbackStatus = feedbackStatus
            };

            request.Sorts = new List<SortDescriptor>()
            {
                new SortDescriptor("OrderDate", ListSortDirection.Descending)
            };
            var items = OrderFeedbackViewModel.GetAll(Db, filter);
            var dataSource = items.ToDataSourceResult(request);
            return new JsonResult { Data = dataSource, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public virtual ActionResult RequestFeedback(string orderId)
        {
            LogI("RequestFeedback, orderId=" + orderId);

            ValueResult<List<string>> result;
            try
            {
                var by = AccessManager.UserId;
                var company = AccessManager.Company;
                var emailService = new EmailService(LogService, SettingsBuilder.GetSmtpSettingsFromCompany(company), AddressService);

                var sendResult = OrderFeedbackViewModel.SendFeedback(Db,
                    emailService,
                    orderId,
                    Time.GetUtcTime(),
                    by);

                if (sendResult.Status == CallStatus.Success)
                {
                    result = ValueResult<List<string>>.Success("Message has been successfully sent", new List<string>() { orderId });
                }
                else
                {
                    result = ValueResult<List<string>>.Error("Can't send email. Details: " + sendResult.Message);
                }
            }
            catch (Exception ex)
            {
                LogE("RequestFeedback", ex);
                result = ValueResult<List<string>>.Error("Can't send email. Error=" + ex.Message);
            }

            return new JsonResult
            {
                Data = result,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public virtual ActionResult RequestFeedbacks(string orderIds)
        {
            LogI("RequestFeedbacks, orderIds=" + orderIds);

            var by = AccessManager.UserId;
            var company = AccessManager.Company;

            ValueResult<List<string>> result = ValueResult<List<string>>.Success(
                "Message has been successfully sent", 
                new List<string>());
            var ids = orderIds.Split(", ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            foreach (var orderId in ids)
            {
                try
                {
                    var emailService = new EmailService(LogService, SettingsBuilder.GetSmtpSettingsFromCompany(company), AddressService);

                    var sendResult = OrderFeedbackViewModel.SendFeedback(Db,
                        emailService,
                        orderId,
                        Time.GetUtcTime(),
                        by);

                    if (sendResult.Status == CallStatus.Success)
                    {
                        result.Data.Add(orderId);
                    }
                }
                catch (Exception ex)
                {
                    LogE("RequestFeedbacks", ex);
                }
            }

            return new JsonResult()
            {
                Data = result,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        
    }
}
