using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Common.Helpers;
using Amazon.Core.Models;

namespace Amazon.Web.ViewModels.Inventory.Counting
{
    public class StyleItemCountingViewModel
    {
        public long Id { get; set; }
        public long? StyleId { get; set; }

        public string Size { get; set; }

        public string Color { get; set; }

        public string Name
        {
            get
            {
                return StringHelper.JoinTwo("/", this.Size, this.Color);
            }
        }


        public int? RemainingQuantity { get; set; }
        public int BoxQuantity { get; set; }

        public string CountingStatus { get; set; }
        public string CountingName { get; set; }
        public DateTime? CountingDate { get; set; }

        public int? ApproveStatus { get; set; }

        public int FormattedApproveStatus
        {
            get
            {
                if (Math.Abs(RemainingQuantity - BoxQuantity ?? 0) < 5)
                    return (int)ApproveStatuses.Approved;

                return ApproveStatus ?? (int)ApproveStatuses.None;
            }
        }

        public string ApproveStatusName
        {
            get { return ApproveStatusHelper.GetName((ApproveStatuses)FormattedApproveStatus); }
        }

        public bool NeedApprove
        {
            get
            {
                if (Math.Abs(RemainingQuantity - BoxQuantity ?? 0) < 5)
                    return false;

                if (String.IsNullOrEmpty(CountingStatus) || CountingStatus == "None")
                    return false;

                if (ApproveStatus == (int) ApproveStatuses.Approved)
                    return false;

                return true;
            }
        }

        public override string ToString()
        {
            return "Id=" + Id + ", CountingStatus=" + CountingStatus + ", CountingName=" + CountingName + ", CountingDate=" + CountingDate;
        }
    }
}