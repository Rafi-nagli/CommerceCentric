using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Contracts.SystemActions;
using Amazon.Core.Models;
using Amazon.Core.Models.SystemActions.SystemActionDatas;
using Newtonsoft.Json;


namespace Amazon.Model.Models
{
    public class SystemActionHelper
    {
        public static void RequestPriceRecalculation(
            IUnitOfWork db,
            ISystemActionService systemAction,
            long listingId,
            long? by)
        {
            systemAction.AddAction(
                db,
                SystemActionType.ListingPriceRecalculation,
                listingId.ToString(),
                null,
                null,
                by);
        }

        public static void RequestItemUpdate(
            IUnitOfWork db,
            ISystemActionService systemAction,
            long itemId,
            long? by)
        {
            systemAction.AddAction(
                db,
                SystemActionType.UpdateOnMarketProductData,
                itemId.ToString(),
                null,
                null,
                by);
        }

        public static void RequestUpdateCache(
            IUnitOfWork db,
            ISystemActionService systemAction,
            long styleId,
            long? by)
        {
            systemAction.AddAction(db,
                SystemActionType.UpdateCache,
                null,
                new UpdateCacheInput()
                {
                    StyleIdList = new List<long>() { styleId },
                },
                null,
                by);
        }

        public static void RequestQuantityDistribution(
            IUnitOfWork db,
            ISystemActionService systemAction,
            long styleId,
            long? by)
        {
            systemAction.AddAction(db,
                SystemActionType.QuantityDistribute,
                null,
                new QuantityDistributeInput()
                {
                    StyleId = styleId,
                },
                null,
                by);
        }

        public static void RequestQuantityDistribution(
                IUnitOfWork db,
                ISystemActionService systemAction,
                long[] styleIdList,
                long? by)
        {
            systemAction.AddAction(db,
                SystemActionType.QuantityDistribute,
                null,
                new QuantityDistributeInput()
                {
                    StyleIdList = styleIdList,
                },
                null,
                by);
        }

        public static void RequestPriceRuleUpdates(
            IUnitOfWork db,
            ISystemActionService systemAction,
            IList<string> skuList,
            long? by)
        {
            foreach (var sku in skuList)
            {
                systemAction.AddAction(db,
                    SystemActionType.UpdateOnMarketProductPriceRule,
                    sku,
                    null,
                    null,
                    by);
            }
        }

        //public static void RequestImageUpdates(
        //    IUnitOfWork db,
        //    ISystemActionService systemAction,
        //    long itemId,
        //    long? by)
        //{
        //    RequestImageUpdates(db, systemAction, new List<long>() { itemId }, by);
        //}

        //public static void RequestImageUpdates(
        //    IUnitOfWork db,
        //    ISystemActionService systemAction,
        //    IList<long> itemIds,
        //    long? by)
        //{
        //    foreach (var itemId in itemIds)
        //    {
        //        systemAction.AddAction(db,
        //            SystemActionType.UpdateOnMarketProductImage,
        //            itemId.ToString(),
        //            null,
        //            null,
        //            by);
        //    }
        //}

        public static void RequestItemTitleUpdates(
            IUnitOfWork db,
            ISystemActionService systemAction,
            IList<int> itemIds,
            long? by)
        {
            foreach (var itemId in itemIds)
            {
                systemAction.AddAction(db,
                    SystemActionType.UpdateOnMarketProductTitle,
                    itemId.ToString(),
                    null,
                    null,
                    by);
            }
        }

        public static void RequestAddTagUpdates(
            IUnitOfWork db,
            ISystemActionService systemAction,
            IList<long> styleIds,
            string tag,
            long? by)
        {
            foreach (var styleId in styleIds)
            {
                systemAction.AddAction(db,
                    SystemActionType.UpdateOnMarketProductTags,
                    styleId.ToString(),
                    new UpdateProductTagsInput()
                    {
                        Action = UpdateProductTagsInput.ADD_ACTION,
                        Tags = new List<string>()
                        {
                            tag
                        }
                    },
                    null,
                    by);
            }
        }

        public static void RequestRemoveTagUpdates(
            IUnitOfWork db,
            ISystemActionService systemAction,
            IList<long> styleIds,
            string tag,
            long? by)
        {
            foreach (var styleId in styleIds)
            {
                systemAction.AddAction(db,
                    SystemActionType.UpdateOnMarketProductTags,
                    styleId.ToString(),
                    new UpdateProductTagsInput()
                    {
                        Action = UpdateProductTagsInput.DELETE_ACTION,
                        Tags = new List<string>()
                        {
                            tag
                        }
                    },
                    null,
                    by);
            }
        }

        public static void RequestItemBarcodeUpdates(
            IUnitOfWork db,
            ISystemActionService systemAction,
            IList<int> itemIds,
            string newBarcode,
            long? by)
        {
            foreach (var itemId in itemIds)
            {
                systemAction.AddAction(db,
                    SystemActionType.UpdateOnMarketProductBarcode,
                    itemId.ToString(),
                    new UpdateProductBarcodeInput()
                    {
                        NewBarcode = newBarcode
                    },
                    null,
                    by);
            }
        }


        public static void RequestRelationshipUpdates(
            IUnitOfWork db,
            ISystemActionService systemAction,
            long itemId,
            long? by)
        {
            systemAction.AddAction(db,
                SystemActionType.UpdateOnMarketProductRelationship,
                itemId.ToString(),
                null,
                null,
                by);
        }

        public static void AddCancelationActionSequences(
            IUnitOfWork db,
            ISystemActionService systemAction,
            long orderId,
            string orderNumber,
            string itemId,
            long? emailId,
            string replyToEmail,
            string replyToSubject,
            string replyToShortBody,
            long? by,
            CancelReasonType reasonType)
        {
            var cancelOrderActionId = systemAction.AddAction(db,
                    SystemActionType.UpdateOnMarketCancelOrder,
                    orderNumber,
                    new CancelOrderInput()
                    {
                        OrderNumber = orderNumber,
                        ItemId = itemId,
                    },
                    null,
                    by);

            if (reasonType == CancelReasonType.PerBuyerRequest)
            {
                systemAction.AddAction(db,
                    SystemActionType.SendEmail,
                    orderNumber,
                    new SendEmailInput()
                    {
                        EmailType = EmailTypes.AcceptOrderCancellationToBuyer,
                        OrderEntityId = orderId,
                        OrderId = orderNumber,
                        ReplyToEmail = replyToEmail,
                        ReplyToSubject = replyToSubject,
                    },
                    cancelOrderActionId,
                    by);

                //systemAction.AddAction(db,
                //    SystemActionType.SendEmail,
                //    orderNumber,
                //    new SendEmailInput()
                //    {
                //        EmailType = EmailTypes.AcceptOrderCancellationToSeller,
                //        OrderEntityId = orderId,
                //        OrderId = orderNumber,
                //        Args = new Dictionary<string, string>()
                //                                {
                //                                                {
                //                                                    "SourceMessage",
                //                                                    replyToShortBody
                //                                                }
                //                                }
                //    },
                //    cancelOrderActionId,
                //    by);
            }

            var commentText = reasonType == CancelReasonType.PerBuyerRequest
                ? "[System] Order was canceled per buyer request"
                : "[System] Manually canceled";

            systemAction.AddAction(db,
                SystemActionType.AddComment,
                orderNumber,
                new AddCommentInput()
                {
                    OrderId = orderId,
                    OrderNumber = orderNumber,
                    Message = commentText,
                    Type = (int)CommentType.ReturnExchange,
                    LinkedEmailId = emailId,
                    By = by,
                },
                cancelOrderActionId,
                by);
        }


        public static string GetName(SystemActionStatus actionType)
        {
            switch (actionType)
            {
                case SystemActionStatus.None:
                    return "Awaiting";
                case SystemActionStatus.Done:
                    return "Done";
                case SystemActionStatus.InProgress:
                    return "In progress";
                case SystemActionStatus.Fail:
                    return "Failed";
                case SystemActionStatus.Suspended:
                    return "Suspended";
            }
            return "-";
        }

        public static T FromStr<T>(string data) where T : ISystemActionParams
        {
            if (data == null)
                data = ""; //or return default(T)

            return JsonConvert.DeserializeObject<T>(data);
        }

        public static string ToStr(ISystemActionOutput data)
        {
            if (data == null)
                return null;

            return JsonConvert.SerializeObject(data);
        }
        
        static public string ToStr(ISystemActionInput data)
        {
            if (data == null)
                return null;

            return JsonConvert.SerializeObject(data);
        }
    }
}
