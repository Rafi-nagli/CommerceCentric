using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Helpers;
using Amazon.Core.Models;
using Amazon.Core.Models.SystemActions.SystemActionDatas;
using Amazon.DTO;
using Amazon.Model.Models;
using Newtonsoft.Json;

namespace Amazon.Web.ViewModels.Mailing
{
    public class RefundViewModel
    {
        public long Id { get; set; }

        public decimal? Amount { get; set; }
        public int? RefundMoney { get; set; }
        public int Status { get; set; }

        public string Message { get; set; }

        public DateTime Date { get; set; }
        public long? By { get; set; }

        public string StatusName
        {
            get { return SystemActionHelper.GetName((SystemActionStatus) Status); }
        }

        public RefundViewModel()
        {
            
        }

        public RefundViewModel(SystemActionDTO action)
        {
            Id = action.Id;

            var input = JsonConvert.DeserializeObject<ReturnOrderInput>(action.InputData);
            if (action.OutputData != null)
            {
                var output = JsonConvert.DeserializeObject<ReturnOrderOutput>(action.OutputData);
                Message = output.ResultMessage;
            }

            Amount = RefundHelper.GetAmount(input);
            RefundMoney = input.RefundMoney;
            Status = action.Status;
            Date = action.CreateDate;
            By = action.CreatedBy;
        }

        public static IList<RefundViewModel> GetByOrderId(IUnitOfWork db,
            string orderNumber)
        {
            var requestQuery = from a in db.SystemActions.GetAllAsDtoWithUser()
                               where a.Tag == orderNumber
                                     && a.Type == (int)SystemActionType.UpdateOnMarketReturnOrder
                               select a;

            var requests = requestQuery.ToList();

            return requests.Select(r => new RefundViewModel(r)).ToList();
        }
    }
}