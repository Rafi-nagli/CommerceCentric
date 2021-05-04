using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.Core.Models
{
    public enum PublishedStatuses
    {
        None = 0,
        New = 1,
        HasChanges = 5,
        HasChangesWithSKU = 6,
        HasChangesWithProductId = 8,

        ChangesSubmited = 7,

        PublishedInProgress = 9,

        PublishingErrors = 25,

        HasPublishRequest = 45, 
        Published = 50,
        
        PublishedInactive = 55, 

        HasUnpublishRequest = 100,
        Unpublished = 105,
    }

    public class PublishedStatusesHelper
    {
        public static int GetIndex(PublishedStatuses status)
        {
            if (status == PublishedStatuses.PublishingErrors)
                return 9;
            if (status == PublishedStatuses.PublishedInProgress)
                return 25;
            return (int)status;
        }

        public static bool InProgressStatus(PublishedStatuses status)
        {
            return status == PublishedStatuses.ChangesSubmited
                || status == PublishedStatuses.HasChanges
                || status == PublishedStatuses.HasChangesWithProductId
                || status == PublishedStatuses.HasChangesWithSKU
                || status == PublishedStatuses.HasPublishRequest
                || status == PublishedStatuses.HasUnpublishRequest
                || status == PublishedStatuses.PublishedInProgress;
        }

        public static string GetName(PublishedStatuses status)
        {
            switch (status) 
            {
                case PublishedStatuses.HasChanges:
                    return "Has unpublished changes";
                case PublishedStatuses.None:
                    return "n/a";
                case PublishedStatuses.New:
                    return "New, unpublished";
                case PublishedStatuses.ChangesSubmited:
                    return "Updates submitted";
                case PublishedStatuses.PublishingErrors:
                    return "Has publish errors";
                case PublishedStatuses.Published:
                    return "Published";
                case PublishedStatuses.PublishedInProgress:
                    return "Publishing In Progress";
                case PublishedStatuses.PublishedInactive:
                    return "Published, marked as inactive";
                case PublishedStatuses.HasChangesWithProductId:
                    return "Changes + UPC override";
                case PublishedStatuses.HasChangesWithSKU:
                    return "Changes + SKU override";


                case PublishedStatuses.HasUnpublishRequest:
                    return "To unpublish";
                case PublishedStatuses.Unpublished:
                    return "Unpublished";
            }
            return "-";
        }
    }
}
