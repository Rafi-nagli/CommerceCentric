using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Helpers;
using Amazon.Core.Models.Calls;
using Amazon.Core.Models.SystemActions;
using Amazon.Model.Models;
using Amazon.Web.Models;

namespace Amazon.Web.ViewModels.Inventory.Boxes
{
    public enum BoxIncomePackType
    {
        PPK = 0,
        PolyBagged = 1,
        Other = 2,
    }


    public class AddBoxWizardViewModel
    {
        public long StyleId { get; set; }
        public string StyleString { get; set; }

        public int IncomeType { get; set; }

        public string BoxBarcode { get; set; }
        public int BoxQuantity { get; set; }
        public int UnitsPerBox { get; set; }
        public bool Printed { get; set; }
        public bool PolyBags { get; set; }
        public bool Owned { get; set; }
        public decimal? Price { get; set; }

        public IList<BoxSizeWizardViewModel> Sizes { get; set; }

        public DateTime? UpdateDate { get; set; }
        public DateTime? CreateDate { get; set; }

        public DateTime? CreateDateUtc
        {
            get { return DateHelper.ConvertAppToUtc(CreateDate); }
            set { CreateDate = DateHelper.ConvertUtcToApp(value); }
        }

        public override string ToString()
        {
            return "StyleId=" + StyleId
                   + ", IncomeType=" + IncomeType
                   + ", BoxBarcode=" + BoxBarcode
                   + ", BoxQuantity=" + BoxQuantity
                   + ", Printed=" + Printed
                   + ", PolyBags=" + PolyBags
                   + ", Owned=" + Owned
                   + ", Price=" + Price
                   + ", Sizes=" + (Sizes == null
                       ? "[null]"
                       : String.Join(",\r\n", Sizes.Select(s => s.ToString()).ToList()));
        }


        public static AddBoxWizardViewModel BuildFromStyleId(IUnitOfWork db,
            long styleId)
        {
            var model = new AddBoxWizardViewModel();

            var style = db.Styles.GetByStyleIdAsDto(styleId);
            model.StyleString = style.StyleID;

            model.StyleId = styleId;

            model.Price = db.OpenBoxes.GetByStyleId(styleId).OrderBy(b => b.Id).Select(b => b.Price).FirstOrDefault();
            model.BoxQuantity = 1;
            model.Owned = true;

            var styleItems = db.StyleItems.GetByStyleIdWithSizeGroupAsDto(styleId).ToList();
            model.Sizes = styleItems
                .OrderBy(si => SizeHelper.GetSizeIndex(si.Size))
                .ThenBy(si => si.Color)
                .Select(si => new BoxSizeWizardViewModel(si))
                .ToList();

            model.SetDefaultBreakdowns(model.Sizes.ToList());

            model.CreateDate = DateHelper.GetAppNowTime().Date;

            model.BoxBarcode = db.SealedBoxes.GetDefaultBoxNameForBothType(styleId, model.CreateDate.Value);

            return model;
        }

        private void SetDefaultBreakdowns(List<BoxSizeWizardViewModel> items)
        {
            if (items.Count < 1)
                return;

            var breakdowns = SizeHelper.GetBreakdowns(items.Select(i => i.Size).ToList());
            if (breakdowns != null)
            {
                for (int i = 0; i < items.Count; i++)
                {
                    items[i].Breakdown = breakdowns[i];
                }
            }
        }

        public List<MessageString> Validate(IUnitOfWork db,
           ILogService log,
           DateTime when)
        {
            var messages = new List<MessageString>();

            if (IncomeType == (int) BoxIncomePackType.PPK)
            {
                var existBoxes = db.SealedBoxes.GetAllAsDto().Where(b => b.StyleId == StyleId
                                                      && b.BoxQuantity == BoxQuantity)
                                                      .ToList();

                foreach (var existBox in existBoxes)
                {
                    var boxItems = db.SealedBoxItems.GetByBoxIdAsDto(existBox.Id);
                    var isEqual = Sizes.Count(s => s.Breakdown > 0) == boxItems.Count;
                    foreach (var boxItem in boxItems)
                    {
                        var size = Sizes.FirstOrDefault(s => s.Id == boxItem.StyleItemId);
                        if (size == null || size.Breakdown != boxItem.BreakDown)
                            isEqual = false;
                    }

                    if (isEqual)
                    {
                        messages.Add(new MessageString()
                        {
                            Message = "A similar boxes already exist (created on " + DateHelper.ToDateString(existBox.CreateDate) + "). Are you sure you would like to create them?",
                            Status = MessageStatus.Info,
                        });

                        return messages;
                    }
                }
            }

            if (IncomeType == (int) BoxIncomePackType.PolyBagged)
            {
                foreach (var size in Sizes.Where(si => si.Quantity > 0))
                {
                    var totalQuantity = UnitsPerBox * size.Quantity;

                    var existBoxes = db.SealedBoxes.GetAllAsDto().Where(b => b.StyleId == StyleId
                                                          && b.BoxQuantity == size.Quantity)
                                                          .ToList();

                    foreach (var existBox in existBoxes)
                    {
                        var boxItems = db.SealedBoxItems.GetByBoxIdAsDto(existBox.Id);
                        var existTotalQuantity = boxItems.Sum(bi => bi.BreakDown) * existBox.BoxQuantity;
                        if (existTotalQuantity == totalQuantity && boxItems.Count == 1)
                        {
                            messages.Add(new MessageString()
                            {
                                Message = "A similar boxes already exist (created on " + DateHelper.ToDateString(existBox.CreateDate) + "). Are you sure you would like to create them?",
                                Status = MessageStatus.Info,
                            });

                            return messages;
                        }
                    }
                }
            }

            if (IncomeType == (int) BoxIncomePackType.Other)
            {
                var existBoxes = db.OpenBoxes.GetAllAsDto().Where(b => b.StyleId == StyleId
                                                      && b.BoxQuantity == 1)
                                                      .ToList();

                foreach (var existBox in existBoxes)
                {
                    var boxItems = db.OpenBoxItems.GetByBoxIdAsDto(existBox.Id);
                    
                    var isEqual = Sizes.Count(s => s.Quantity > 0) == boxItems.Count;
                    foreach (var boxItem in boxItems)
                    {
                        var size = Sizes.FirstOrDefault(s => s.Id == boxItem.StyleItemId);
                        if (size == null || size.Quantity != boxItem.Quantity)
                            isEqual = false;
                    }

                    if (isEqual)
                    {
                        messages.Add(new MessageString()
                        {
                            Message = "A similar box already exists (created on " +
                                DateHelper.ToDateString(existBox.CreateDate) + "). Are you sure you would like to create one?",
                            Status = MessageStatus.Info,
                        });

                        return messages;
                    }
                }
            }

            return messages;
        }

        public void Apply(IUnitOfWork db, 
            IQuantityManager quantityManager,
            ILogService log,
            ICacheService cache,
            ISystemActionService actionService,
            DateTime when,
            long? by)
        {
            log.Info("AddSealedBoxWizardViewModel.Apply, StyleId=" + StyleId);

            if (IncomeType == (int) BoxIncomePackType.PPK)
            {
                var box = new SealedBoxViewModel();
                box.StyleId = StyleId;
                box.BoxBarcode = BoxBarcode;
                box.BoxQuantity = BoxQuantity;
                box.Price = Price ?? 0;
                
                box.Owned = Owned;
                box.PolyBags = false;
                box.Printed = Printed;

                box.CreateDate = CreateDate.HasValue ? DateHelper.ConvertUtcToApp(CreateDate.Value.ToUniversalTime()) : when;

                box.StyleItems = new StyleItemCollection()
                {
                    Items = Sizes.Select(s => new StyleItemViewModel()
                    {
                        Id = s.Id,
                        Breakdown = s.Breakdown,
                    }).ToList()
                };

                box.Apply(db, quantityManager, when, by);
            }

            if (IncomeType == (int) BoxIncomePackType.PolyBagged)
            {
                foreach (var size in Sizes.Where(si => si.Quantity > 0))
                {
                    var box = new SealedBoxViewModel();
                    box.StyleId = StyleId;
                    box.BoxBarcode = BoxBarcode;
                    box.BoxQuantity = size.Quantity ?? 0;
                    box.Price = Price ?? 0;

                    box.Owned = Owned;
                    box.PolyBags = true;
                    box.Printed = Printed;

                    box.CreateDate = CreateDate.HasValue ? DateHelper.ConvertUtcToApp(CreateDate.Value.ToUniversalTime()) : when;

                    box.StyleItems = new StyleItemCollection()
                    {
                        Items = new List<StyleItemViewModel>()
                        {
                            new StyleItemViewModel()
                            {
                                Id = size.Id,
                                Breakdown = UnitsPerBox
                            }
                        }
                    };

                    box.Apply(db, quantityManager, when, by);
                }
            }

            if (IncomeType == (int)BoxIncomePackType.Other)
            {
                var box = new OpenBoxViewModel();
                box.StyleId = StyleId;
                box.BoxBarcode = BoxBarcode;
                box.BoxQuantity = 1;
                box.Price = Price ?? 0;

                box.Owned = Owned;
                box.PolyBags = false;
                box.Printed = Printed;

                box.CreateDate = CreateDate.HasValue ? DateHelper.ConvertUtcToApp(CreateDate.Value.ToUniversalTime()) : when;

                box.StyleItems = new StyleItemCollection()
                {
                    Items = Sizes.Select(s => new StyleItemViewModel()
                    {
                        Id = s.Id,
                        Quantity = s.Quantity,
                    }).ToList()
                };

                box.Apply(db, quantityManager, when, by);
            }

            foreach (var size in Sizes)
            {
                var styleItem = db.StyleItems.Get(size.Id);
                if (size.UseBoxQuantity
                    && styleItem.Quantity.HasValue)
                {
                    log.Info("Switch to box quantity, styleItemId=" + size.Id);

                    var oldQuantity = styleItem.Quantity;

                    styleItem.Quantity = null;
                    styleItem.QuantitySetBy = null;
                    styleItem.QuantitySetDate = null;
                    styleItem.RestockDate = null;

                    db.Commit();

                    quantityManager.LogStyleItemQuantity(db,
                        styleItem.Id,
                        null,
                        oldQuantity,
                        QuantityChangeSourceType.UseBoxQuantity,
                        null,
                        null,
                        BoxBarcode,
                        when,
                        by);
                }
            }

            cache.RequestStyleIdUpdates(db,
                new List<long>() { StyleId },
                UpdateCacheMode.IncludeChild,
                by);

            SystemActionHelper.RequestQuantityDistribution(db, actionService, StyleId, by);
        }
    }
}