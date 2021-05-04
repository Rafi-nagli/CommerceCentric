using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models
{
    public enum PublishFeedModes
    {
        Publish = 0,
        UnPublish = 2,

        Hold = 5,
        UnHold = 6,

        HoldStyle = 10,
        UnHoldStyle = 11,
    }

    public static class PublishFeedModeHelper
    {
        public static string GetName(PublishFeedModes mode)
        {
            if (mode == PublishFeedModes.Publish)
                return "Publish/Repricing";
            if (mode == PublishFeedModes.UnPublish)
                return "Unpublish";
            if (mode == PublishFeedModes.Hold)
                return "Hold";
            if (mode == PublishFeedModes.UnHold)
                return "Unhold";
            if (mode == PublishFeedModes.HoldStyle)
                return "Hold styles";
            if (mode == PublishFeedModes.UnHoldStyle)
                return "Unhold styles";
            return "n/a";
        }
    }
}
