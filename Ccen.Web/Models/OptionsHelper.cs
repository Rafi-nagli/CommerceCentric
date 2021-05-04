using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Amazon.Common.Helpers;
using Amazon.Core.Models;
using Amazon.Core.Models.DropShippers;
using Amazon.Core.Models.Enums;
using Amazon.Core.Models.Items;
using Amazon.DAL;
using Amazon.DTO.Sizes;
using Amazon.Model.Implementation;
using Amazon.Model.Implementation.Markets;

namespace Amazon.Web.Models
{
    public enum MarketplaceType
    {
        Amazon = 1,
        EBay = 2
    }

    public class Constants
    {
        public const string DefaultCountryCode = "US";
    }

    public class OptionsHelper
    {
        public static List<KeyValuePair<string, int?>> ColorCodeCorrespondence = new List<KeyValuePair<string, int?>>
        {
            new KeyValuePair<string, int?>("Select...", null),
            new KeyValuePair<string, int?>("Black", 1),
            new KeyValuePair<string, int?>("Blue-dark", 3),
            new KeyValuePair<string, int?>("Blue-light", 2),
            new KeyValuePair<string, int?>("Brown", 4),
            new KeyValuePair<string, int?>("Gray", 6),
            new KeyValuePair<string, int?>("Green", 5),
            new KeyValuePair<string, int?>("Multi-color", 0),
            new KeyValuePair<string, int?>("Orange", 7),
            new KeyValuePair<string, int?>("Pink", 8),
            new KeyValuePair<string, int?>("Purple", 9),
            new KeyValuePair<string, int?>("Red", 10),
            new KeyValuePair<string, int?>("White", 11),
            new KeyValuePair<string, int?>("Yellow", 12)
        };

        public static SelectList Colors
        {
            get
            {
                return new SelectList(ColorCodeCorrespondence, "Value", "Key");
            }

        }

        public static SelectList SalesPeriods
        {
            get
            {
                return new SelectList(SalesPeriodsCorrespondence, "Value", "Key");
            }
        }

        public static List<KeyValuePair<string, int>> SalesPeriodsCorrespondence = new List<KeyValuePair<string, int>>
        {
            new KeyValuePair<string, int>("Overall", 0),
            new KeyValuePair<string, int>("Week", 1),
            new KeyValuePair<string, int>("Two Weeks", 2),
            new KeyValuePair<string, int>("Two Months", 3),
            new KeyValuePair<string, int>("Year", 4)
        };



        public static SelectList OnHoldModeList
        {
            get
            {
                return new SelectList(OnHoldModesAsArray, "Value", "Key");
            }
        }

        public static List<KeyValuePair<string, int>> OnHoldModesAsArray = new List<KeyValuePair<string, int>>
        {
            new KeyValuePair<string, int>(OnHoldModeHelper.GetName(OnHoldModes.None), (int)OnHoldModes.None),
            new KeyValuePair<string, int>(OnHoldModeHelper.GetName(OnHoldModes.OnListing), (int)OnHoldModes.OnListing),
            new KeyValuePair<string, int>(OnHoldModeHelper.GetName(OnHoldModes.OnStyle), (int)OnHoldModes.OnStyle),
            new KeyValuePair<string, int>(OnHoldModeHelper.GetName(OnHoldModes.OnBoth), (int)OnHoldModes.OnBoth),
            new KeyValuePair<string, int>(OnHoldModeHelper.GetName(OnHoldModes.OnListingOrStyle), (int)OnHoldModes.OnListingOrStyle),
            new KeyValuePair<string, int>(OnHoldModeHelper.GetName(OnHoldModes.OnListingWithActiveStyle), (int)OnHoldModes.OnListingWithActiveStyle),
        };


        public static SelectList BuyBoxWinModeList
        {
            get
            {
                return new SelectList(BuyBoxWinModesAsArray, "Value", "Key");
            }
        }

        public static List<KeyValuePair<string, int>> BuyBoxWinModesAsArray = new List<KeyValuePair<string, int>>
        {
            new KeyValuePair<string, int>(BuyBoxWinModeHelper.GetName(BuyBoxWinModes.Win), (int)BuyBoxWinModes.Win),
            new KeyValuePair<string, int>(BuyBoxWinModeHelper.GetName(BuyBoxWinModes.NotWin), (int)BuyBoxWinModes.NotWin),
        };



        public static SelectList CommentTypes
        {
            get { return new SelectList(CommentTypesAsArray, "Value", "Key"); }
        }

        public static List<KeyValuePair<string, int>> CommentTypesAsArray = new List<KeyValuePair<string, int>>
        {
            new KeyValuePair<string, int>(CommentHelper.TypeToString(CommentType.None), (int)CommentType.None),
            new KeyValuePair<string, int>(CommentHelper.TypeToString(CommentType.Address), (int)CommentType.Address),
            new KeyValuePair<string, int>(CommentHelper.TypeToString(CommentType.Notification), (int)CommentType.Notification),
            new KeyValuePair<string, int>(CommentHelper.TypeToString(CommentType.IncomingEmail), (int)CommentType.IncomingEmail),
            new KeyValuePair<string, int>(CommentHelper.TypeToString(CommentType.OutputEmail), (int)CommentType.OutputEmail),
            new KeyValuePair<string, int>(CommentHelper.TypeToString(CommentType.ReturnExchange), (int)CommentType.ReturnExchange),
            new KeyValuePair<string, int>(CommentHelper.TypeToString(CommentType.MarketplaceClaim), (int)CommentType.MarketplaceClaim),
            new KeyValuePair<string, int>(CommentHelper.TypeToString(CommentType.Other), (int)CommentType.Other)
        };

        public static SelectList BatchTimeStatusList
        {
            get { return new SelectList(BatchTimeStatusAsArray, "Value", "Key"); }
        }

        public static List<KeyValuePair<string, int>> BatchTimeStatusAsArray = new List<KeyValuePair<string, int>>
        {
            new KeyValuePair<string, int>(BatchTimeStatusHelper.GetStatusName(BatchTimeStatus.BeforeFirst), (int)BatchTimeStatus.BeforeFirst),
            new KeyValuePair<string, int>(BatchTimeStatusHelper.GetStatusName(BatchTimeStatus.AfterFirstBeforeSecond), (int)BatchTimeStatus.AfterFirstBeforeSecond),
            new KeyValuePair<string, int>(BatchTimeStatusHelper.GetStatusName(BatchTimeStatus.AfterSecond), (int)BatchTimeStatus.AfterSecond),
        };

        public static SelectList CountPersonList
        {
            get { return new SelectList(CountPersonAsArray, "Value", "Key"); }
        }

        public static List<KeyValuePair<string, string>> CountPersonAsArray = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("None", ""),
            new KeyValuePair<string, string>("Artem", "Artem"),
            new KeyValuePair<string, string>("Evgenij", "Evgenij"),
            new KeyValuePair<string, string>("Valentin", "Valentin"),
            new KeyValuePair<string, string>("Tony", "Tony"),
        };


        public static SelectList LiteCountingByNameList
        {
            get { return new SelectList(LiteCountingByNameAsArray, "Value", "Key"); }
        }

        public static List<KeyValuePair<string, string>> LiteCountingByNameAsArray = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("None", ""),
            new KeyValuePair<string, string>("Sasha", "Sasha"),
            new KeyValuePair<string, string>("Sveta", "Sveta"),
            new KeyValuePair<string, string>("Valya", "Valya"),
        };


        public static SelectList LiteCountingStatusList
        {
            get { return new SelectList(LiteCountingStatusAsArray, "Value", "Key"); }
        }

        public static List<KeyValuePair<string, string>> LiteCountingStatusAsArray = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>(CountingStatusesEx.None, ""),
            //new KeyValuePair<string, string>("NR", "NR"),
            new KeyValuePair<string, string>(CountingStatusesEx.Reviewed, CountingStatusesEx.Reviewed),
            new KeyValuePair<string, string>(CountingStatusesEx.Counted, CountingStatusesEx.Counted),
            new KeyValuePair<string, string>(CountingStatusesEx.Lost, CountingStatusesEx.Lost),
            new KeyValuePair<string, string>(CountingStatusesEx.Recount, CountingStatusesEx.Recount),
            new KeyValuePair<string, string>(CountingStatusesEx.Verified, CountingStatusesEx.Verified),
        };


        public static SelectList EmailTypeList
        {
            get
            {
                return new SelectList(EmailTypeAsArray, "Value", "Key");
            }
        }

        public static List<KeyValuePair<string, int>> EmailTypeAsArray = new List<KeyValuePair<string, int>>()
        {
            new KeyValuePair<string, int>("Custom", (int)EmailTypes.CustomEmail),
            new KeyValuePair<string, int>("Verify Address", (int)EmailTypes.AddressVerify),
            new KeyValuePair<string, int>("Address Changed", (int)EmailTypes.AddressChanged),
            new KeyValuePair<string, int>("Already Shipped", (int)EmailTypes.AlreadyShipped),
            new KeyValuePair<string, int>("Lost Package", (int)EmailTypes.LostPackage),
            new KeyValuePair<string, int>("Lost Package 2", (int)EmailTypes.LostPackage2),
            new KeyValuePair<string, int>("Undeliverable Inquiry", (int)EmailTypes.UndeliverableInquiry),
            new KeyValuePair<string, int>("Notice Left", (int)EmailTypes.NoticeLeft),
            new KeyValuePair<string, int>("Undeliverable", (int)EmailTypes.UndeliverableAsAddressed),
            new KeyValuePair<string, int>("Request Feedback", (int)EmailTypes.RequestFeedback),
            new KeyValuePair<string, int>("Exchange Instructions", (int)EmailTypes.ExchangeInstructions),
            new KeyValuePair<string, int>("Return Instructions", (int)EmailTypes.ReturnInstructions),
            new KeyValuePair<string, int>("Damaged Item", (int)EmailTypes.DamagedItem),
            new KeyValuePair<string, int>("Return Wrong/Damaged", (int)EmailTypes.ReturnWrongDamagedItem),
            new KeyValuePair<string, int>("Return Period Expired", (int)EmailTypes.ReturnPeriodExpired),
            new KeyValuePair<string, int>("Not ours", (int)EmailTypes.NotOurs),
            new KeyValuePair<string, int>("Oversold", (int)EmailTypes.Oversold),
            new KeyValuePair<string, int>("Gift Receipt", (int)EmailTypes.GiftReceipt),
        };


        public static SelectList Countries
        {
            get
            {
                using (var db = new UnitOfWork(null))
                {
                    return new SelectList(db.Countries.GetAll().OrderBy(c => c.Order).Select(c => new { Value = c.CountryCode2, Text = c.CountryName }).ToList(), "Value", "Text");
                }
            }
        }

        public static SelectList States
        {
            get
            {
                using (var db = new UnitOfWork(null))
                {
                    return new SelectList(db.States.GetAll().Where(st => st.CountryCode == "US").OrderBy(s => s.StateCode).Select(s => new { Value = s.StateCode, Text = s.StateName}).ToList(), "Value", "Text");
                }
            }
        }

        public static SelectList Roles
        {
            get
            {
                using (var db = new UnitOfWork(null))
                {
                    return new SelectList(db.Roles.GetAllAsDto().OrderBy(s => s.Id).ToList(), "Id", "Name");
                }
            }
        }

        public static SelectList SizeItems
        {
            get
            {
                using (var db = new UnitOfWork(null))
                {
                    return new SelectList(db.Context.Sizes.Select(s => s.Name).Distinct().ToList()
                        .Select(s => new {Text = s, Value = s}).ToList(), "Value", "Text");
                }
            }
        }

        public static SelectList ItemTypes
        {
            get
            {
                return new SelectList(ItemTypesAsArray.Select(s => new { Text = s.Name, Value = s.Id }).ToList(), "Value", "Text");
            }
        }

        public static IList<ItemTypeDTO> ItemTypesAsArray
        {
            get
            {
                using (var db = new UnitOfWork(null))
                {
                    return db.Context.ItemTypes.Select(s => new ItemTypeDTO() { Id = s.Id, Name = s.Name }).ToList();
                }
            }
        }

        public static List<KeyValuePair<string, string>> CarrierListAsArray = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("USPS", "USPS"),
            new KeyValuePair<string, string>("DHL", "DHL"),
            new KeyValuePair<string, string>("FedEx", "FedEx"),
            new KeyValuePair<string, string>("UPS", "UPS"),
            new KeyValuePair<string, string>("Other", "Other"),
        };

        public static SelectList CarrierList
        {
            get
            {
                return new SelectList(CarrierListAsArray, "Value", "Key");
            }
        }

        public static SelectList PublishedStatusList
        {
            get
            {
                return new SelectList(PublishedStatusAsArray, "Value", "Key");
            }
        }

        public static List<KeyValuePair<string, int>> PublishedStatusAsArray = new List<KeyValuePair<string, int>>
        {
            new KeyValuePair<string, int>(PublishedStatusesHelper.GetName(PublishedStatuses.None), (int)PublishedStatuses.None),
            new KeyValuePair<string, int>(PublishedStatusesHelper.GetName(PublishedStatuses.ChangesSubmited), (int)PublishedStatuses.ChangesSubmited),
            new KeyValuePair<string, int>(PublishedStatusesHelper.GetName(PublishedStatuses.HasChanges), (int)PublishedStatuses.HasChanges),
            new KeyValuePair<string, int>(PublishedStatusesHelper.GetName(PublishedStatuses.Published), (int)PublishedStatuses.Published),
            new KeyValuePair<string, int>(PublishedStatusesHelper.GetName(PublishedStatuses.PublishedInProgress), (int)PublishedStatuses.PublishedInProgress),
            new KeyValuePair<string, int>(PublishedStatusesHelper.GetName(PublishedStatuses.PublishedInactive), (int)PublishedStatuses.PublishedInactive),
            new KeyValuePair<string, int>(PublishedStatusesHelper.GetName(PublishedStatuses.PublishingErrors), (int)PublishedStatuses.PublishingErrors),
            new KeyValuePair<string, int>(PublishedStatusesHelper.GetName(PublishedStatuses.HasChangesWithProductId), (int)PublishedStatuses.HasChangesWithProductId),
            new KeyValuePair<string, int>(PublishedStatusesHelper.GetName(PublishedStatuses.HasChangesWithSKU), (int)PublishedStatuses.HasChangesWithSKU),
            new KeyValuePair<string, int>(PublishedStatusesHelper.GetName(PublishedStatuses.New), (int)PublishedStatuses.New),
            new KeyValuePair<string, int>(PublishedStatusesHelper.GetName(PublishedStatuses.Unpublished), (int)PublishedStatuses.Unpublished),
            new KeyValuePair<string, int>(PublishedStatusesHelper.GetName(PublishedStatuses.HasUnpublishRequest), (int)PublishedStatuses.HasUnpublishRequest),
        };

        public static SelectList FillingStatuses
        {
            get { return new SelectList(FillingStatusesAsArray, "Value", "Key"); }
        }

        public static List<KeyValuePair<string, int>> FillingStatusesAsArray = new List<KeyValuePair<string, int>>
        {
            new KeyValuePair<string, int>(FillingStyleStatusesHelper.GetName(FillingStyleStatuses.None), (int)FillingStyleStatuses.None),
            new KeyValuePair<string, int>(FillingStyleStatusesHelper.GetName(FillingStyleStatuses.Temporary), (int)FillingStyleStatuses.Temporary),
            new KeyValuePair<string, int>(FillingStyleStatusesHelper.GetName(FillingStyleStatuses.Basic), (int)FillingStyleStatuses.Basic),
            new KeyValuePair<string, int>(FillingStyleStatusesHelper.GetName(FillingStyleStatuses.Enhanced), (int)FillingStyleStatuses.Enhanced),
            new KeyValuePair<string, int>(FillingStyleStatusesHelper.GetName(FillingStyleStatuses.AllDataEntered), (int)FillingStyleStatuses.AllDataEntered),
            new KeyValuePair<string, int>(FillingStyleStatusesHelper.GetName(FillingStyleStatuses.Updated), (int)FillingStyleStatuses.Updated),
            new KeyValuePair<string, int>(FillingStyleStatusesHelper.GetName(FillingStyleStatuses.Done), (int)FillingStyleStatuses.Done),
        };

        public static SelectList ImageCategories
        {
            get { return new SelectList(ImageCategoriesAsArray, "Value", "Key"); }
        }

        public static List<KeyValuePair<string, int>> ImageCategoriesAsArray = new List<KeyValuePair<string, int>>
        {
            new KeyValuePair<string, int>(StyleImageCategoryHelper.GetName(StyleImageCategories.None), (int)StyleImageCategories.None),
            new KeyValuePair<string, int>(StyleImageCategoryHelper.GetName(StyleImageCategories.Flat), (int)StyleImageCategories.Flat),
            new KeyValuePair<string, int>(StyleImageCategoryHelper.GetName(StyleImageCategories.FlatMain), (int)StyleImageCategories.FlatMain),
            new KeyValuePair<string, int>(StyleImageCategoryHelper.GetName(StyleImageCategories.Live), (int)StyleImageCategories.Live),
            new KeyValuePair<string, int>(StyleImageCategoryHelper.GetName(StyleImageCategories.LiveMain), (int)StyleImageCategories.LiveMain),
            new KeyValuePair<string, int>(StyleImageCategoryHelper.GetName(StyleImageCategories.SizeChart), (int)StyleImageCategories.SizeChart),
            new KeyValuePair<string, int>(StyleImageCategoryHelper.GetName(StyleImageCategories.Swatch), (int)StyleImageCategories.Swatch),
            new KeyValuePair<string, int>(StyleImageCategoryHelper.GetName(StyleImageCategories.Deleted), (int)StyleImageCategories.Deleted),
        };


        public static SelectList PictureStatuses
        {
            get { return new SelectList(PictureStatusesAsArray, "Value", "Key"); }
        }

        public static List<KeyValuePair<string, int>> PictureStatusesAsArray = new List<KeyValuePair<string, int>>
        {
            new KeyValuePair<string, int>("None", (int)StylePictureStatuses.None),
            new KeyValuePair<string, int>("From Marketplace", (int)StylePictureStatuses.FromMarketplace),
            new KeyValuePair<string, int>("No Picture", (int)StylePictureStatuses.NoPicture),
            new KeyValuePair<string, int>("Pic found Internet", (int)StylePictureStatuses.PicFoundInternet),
            new KeyValuePair<string, int>("To Be Photographed", (int)StylePictureStatuses.ToBePhotographed),
            new KeyValuePair<string, int>("Given to photographer", (int)StylePictureStatuses.GivenToPhotographer),
            new KeyValuePair<string, int>("Pic Updated", (int)StylePictureStatuses.PicUpdated),
            new KeyValuePair<string, int>("Pic Send to Amazon", (int)StylePictureStatuses.PicSendToAmazon),
            new KeyValuePair<string, int>("Created support case", (int)StylePictureStatuses.CreatedSupportCase),
            new KeyValuePair<string, int>("Done", (int)StylePictureStatuses.Done),
        };

        public static SelectList DropShipperList
        {
            get
            {
                using (var db = new UnitOfWork(null))
                {
                    var dropShipperList = db.DropShippers.GetAllAsDto().Where(i => i.IsActive).OrderBy(i => i.SortOrder).ToList();
                    return new SelectList(dropShipperList, "Id", "Name");
                }
            }
        }


        public static SelectList DSProductTypeList
        {
            get { return new SelectList(DSProductTypeAsArray, "Value", "Key"); }
        }

        public static List<KeyValuePair<string, int>> DSProductTypeAsArray = new List<KeyValuePair<string, int>>
        {
            new KeyValuePair<string, int>(DSProductTypeHelper.GetName(DSProductType.Watches), (int)DSProductType.Watches),
            new KeyValuePair<string, int>(DSProductTypeHelper.GetName(DSProductType.Jewelry), (int)DSProductType.Jewelry),
            new KeyValuePair<string, int>(DSProductTypeHelper.GetName(DSProductType.Sunglasses), (int)DSProductType.Sunglasses),
        };


        public static SelectList DSFeedTypeList
        {
            get { return new SelectList(DSProductTypeAsArray, "Value", "Key"); }
        }

        public static List<KeyValuePair<string, int>> DSFeedTypeAsArray = new List<KeyValuePair<string, int>>
        {
            new KeyValuePair<string, int>(DSFileTypeHelper.GetName(DSFileTypes.ItemsFull), (int)DSFileTypes.ItemsFull),
            new KeyValuePair<string, int>(DSFileTypeHelper.GetName(DSFileTypes.ItemsLite), (int)DSFileTypes.ItemsLite),
        };

        public static SelectList ActiveBatches
        {
            get
            {
                using (var db = new UnitOfWork(null))
                {
                    var batches = db.OrderBatches.GetBatchesToDisplay(false).OrderByDescending(b => b.CreateDate).ToList();
                    var list = new List<KeyValuePair<long?, string>>(batches.Select(b => new KeyValuePair<long?, string> (b.Id, b.Name)).ToList());
                    return new SelectList(list, "Key", "Value");
                }
            }
        }

        public static SelectList MarketList
        {
            get
            {
                var markets = new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("All", "0" + "_" + ""),
                };
                using (var db = new UnitOfWork(null))
                {
                    var dtoMarketplaces = db.Marketplaces.GetAllAsDto()
                        .Where(m => m.IsActive
                            && !m.IsHidden)
                        .OrderBy(m => m.SortOrder)
                        .ToList();
                    foreach (var market in dtoMarketplaces)
                        markets.Add(new KeyValuePair<string, string>(
                            MarketHelper.GetMarketName((int)market.Market, market.MarketplaceId),
                            ((int)market.Market) + "_" + market.MarketplaceId));
                }
                return new SelectList(markets, "Value", "Key");
            }
        }


        public static SelectList AmazonMarketList
        {
            get
            {
                var markets = new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("All", "0" + "_" + ""),
                };
                using (var db = new UnitOfWork(null))
                {
                    var dtoMarketplaces = db.Marketplaces.GetAllAsDto()
                        .Where(m => m.IsActive
                            && !m.IsHidden
                            && m.Market == (int)MarketType.Amazon)
                        .OrderBy(m => m.SortOrder)
                        .ToList();
                    foreach (var market in dtoMarketplaces)
                        markets.Add(new KeyValuePair<string, string>(
                            MarketHelper.GetMarketName((int)market.Market, market.MarketplaceId),
                            ((int)market.Market) + "_" + market.MarketplaceId));
                }
                return new SelectList(markets, "Value", "Key");
            }
        }
    }
}