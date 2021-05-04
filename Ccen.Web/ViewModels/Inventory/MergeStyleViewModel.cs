using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Models.Calls;
using Amazon.Core.Models.SystemActions;
using Amazon.Web.ViewModels.Messages;
using Amazon.Web.ViewModels.Results;

namespace Amazon.Web.ViewModels.Inventory
{
    public class MergeStyleViewModel
    {
        public string InputMainStyleId { get; set; }
        public string InputSecondStyleId { get; set; }

        public MergeStyleViewModel()
        {
            
        }

        public IList<MessageString> Validate()
        {
            return new List<MessageString>();
        } 

        public bool Merge(ILogService log, 
            IUnitOfWork db,
            ICacheService cacheService,
            DateTime? when,
            long? by,
            out IList<MessageString> messages)
        {
            messages = new List<MessageString>();

            if (String.IsNullOrEmpty(InputMainStyleId))
            {
                messages.Add(MessageString.Error("MainStyleId", "Empty main styleId"));
                return false;
            }

            if (String.IsNullOrEmpty(InputSecondStyleId))
            {
                messages.Add(MessageString.Error("SecondStyleId", "Empty second styleId"));
                return false;
            }

            var results = new List<MessageString>();
            var styleTo = db.Styles.GetAll().FirstOrDefault(s => s.StyleID == InputMainStyleId && !s.Deleted);
            var styleFrom = db.Styles.GetAll().FirstOrDefault(s => s.StyleID == InputSecondStyleId && !s.Deleted);

            if (styleTo == null)
            {
                messages.Add(MessageString.Error("MainStyleId", "Not found main style"));
                return false;
            }

            if (styleFrom == null)
            {
                messages.Add(MessageString.Error("MainStyleId", "Not found second style"));
                return false;
            }

            //Move/update styleItems
            var styleItemToList = db.StyleItems.GetAll().Where(si => si.StyleId == styleTo.Id).ToList();
            var styleItemFromList = db.StyleItems.GetAll().Where(si => si.StyleId == styleFrom.Id).ToList();

            foreach (var toMoveItem in styleItemFromList)
            {
                var toMoveListings = db.Items.GetAll().Where(i => i.StyleItemId == toMoveItem.Id).ToList();
                var toMoveOrderItems = db.OrderItems.GetAll().Where(b => b.StyleItemId == toMoveItem.Id || b.SourceStyleItemId == toMoveItem.Id).ToList();
                var toMoveSourceOrderItems = db.OrderItemSources.GetAll().Where(b => b.StyleItemId == toMoveItem.Id).ToList();

                var existStyleItem = styleItemToList.FirstOrDefault(si => si.SizeId == toMoveItem.SizeId);
                if (existStyleItem == null)
                {
                    toMoveItem.StyleId = styleTo.Id;

                    foreach (var toMoveListing in toMoveListings)
                    {
                        toMoveListing.StyleId = styleTo.Id;
                    }

                    toMoveOrderItems.ForEach(i =>
                    {
                        if (i.StyleId == styleFrom.Id)
                        {
                            i.StyleId = styleTo.Id;
                            i.StyleString = styleTo.StyleID;
                        }
                        log.Info("Moved orderItem=" + i.Id);
                    });
                    toMoveSourceOrderItems.ForEach(i =>
                    {
                        i.StyleId = styleTo.Id;
                        i.StyleString = styleTo.StyleID;
                        log.Info("Moved source orderItem=" + i.Id);
                    });

                    log.Info("Moved whole styleItem=" + toMoveItem.Size);
                }
                else
                {
                    if (!existStyleItem.Weight.HasValue)
                        existStyleItem.Weight = toMoveItem.Weight;
                    if (!existStyleItem.MinPrice.HasValue)
                        existStyleItem.MinPrice = toMoveItem.MinPrice;
                    if (!existStyleItem.MaxPrice.HasValue)
                        existStyleItem.MaxPrice = toMoveItem.MaxPrice;

                    var toMoveBarcodes = db.StyleItemBarcodes.GetAll().Where(b => b.StyleItemId == toMoveItem.Id).ToList();
                    foreach (var toMoveBarcode in toMoveBarcodes)
                    {
                        toMoveBarcode.StyleItemId = existStyleItem.Id;
                        log.Info("Moved barcode=" + toMoveBarcode.Barcode);
                    }

                    var toMoveSpecialCases = db.QuantityChanges.GetAll().Where(b => b.StyleItemId == toMoveItem.Id).ToList();
                    foreach (var toMoveSpecialCase in toMoveSpecialCases)
                    {
                        toMoveSpecialCase.StyleItemId = existStyleItem.Id;
                        toMoveSpecialCase.StyleId = existStyleItem.StyleId;
                        log.Info("Moved special case, id=" + toMoveSpecialCase.Id + ", quantity=" + toMoveSpecialCase.Quantity);
                    }

                    //Move/update listings
                    foreach (var toMoveListing in toMoveListings)
                    {
                        toMoveListing.StyleId = existStyleItem.StyleId;
                        toMoveListing.StyleItemId = existStyleItem.Id;
                        log.Info("Moved listing, id=" + toMoveListing.Id);
                    }

                    toMoveOrderItems.ForEach(i =>
                    {
                        if (i.StyleId == styleFrom.Id)
                        {
                            i.StyleId = styleTo.Id;
                            i.StyleString = styleTo.StyleID;
                        }
                        if (i.StyleItemId == toMoveItem.Id)
                            i.StyleItemId = existStyleItem.Id;
                        if (i.SourceStyleItemId == toMoveItem.Id)
                            i.SourceStyleItemId = existStyleItem.Id;
                        log.Info("Moved orderItem=" + i.Id);
                    });
                    toMoveSourceOrderItems.ForEach(i =>
                    {
                        i.StyleId = styleTo.Id;
                        i.StyleString = styleTo.StyleID;
                        i.StyleItemId = existStyleItem.Id;
                        log.Info("Moved source orderItem=" + i.Id);
                    });

                    //Move/update box items
                    var toMoveOpenBoxItems = db.OpenBoxItems
                        .GetAll()
                        .Where(b => b.StyleItemId == toMoveItem.Id)
                        .ToList();

                    var toMoveSealedBoxItems = db.SealedBoxItems
                        .GetAll()
                        .Where(b => b.StyleItemId == toMoveItem.Id)
                        .ToList();

                    foreach (var openBoxItem in toMoveOpenBoxItems)
                    {
                        openBoxItem.StyleItemId = existStyleItem.Id;
                    }

                    foreach (var sealedBoxItem in toMoveSealedBoxItems)
                    {
                        sealedBoxItem.StyleItemId = existStyleItem.Id;
                    }
                }
            }
            db.Commit();

            //Move locations
            var style1Locations = db.StyleLocations.GetAll().Where(l => l.StyleId == styleTo.Id).ToList();
            var style2Locations = db.StyleLocations.GetAll().Where(l => l.StyleId == styleFrom.Id).ToList();

            var existDefaultLocation = style1Locations.Any(l => l.IsDefault);
            foreach (var toMoveLocation in style2Locations)
            {
                var existLocation = style1Locations.FirstOrDefault(l => l.Isle == toMoveLocation.Isle
                                                                            && l.Section == toMoveLocation.Section
                                                                            && l.Shelf == toMoveLocation.Shelf);
                if (existLocation == null)
                {
                    toMoveLocation.StyleId = styleTo.Id;
                    if (existDefaultLocation)
                        toMoveLocation.IsDefault = false;

                    log.Info(String.Format("Moved location, isle={0}, section={1}, shelf={2}, isDefault={3}",
                        toMoveLocation.Isle,
                        toMoveLocation.Section,
                        toMoveLocation.Shelf,
                        toMoveLocation.IsDefault));
                }
            }
            db.Commit();


            //Move/update features
            var style1Features = db.StyleFeatureValues.GetAll().Where(f => f.StyleId == styleTo.Id).ToList();
            var style2Features = db.StyleFeatureValues.GetAll().Where(f => f.StyleId == styleFrom.Id).ToList();
            foreach (var toMoveFeature in style2Features)
            {
                var existFeature = style1Features.FirstOrDefault(f => f.FeatureId == toMoveFeature.FeatureId);
                if (existFeature == null)
                {
                    toMoveFeature.StyleId = styleTo.Id;
                }
            }
            db.Commit();
            

            //Boxes
            var style2SealedBoxes = db.SealedBoxes.GetAll().Where(b => b.StyleId == styleFrom.Id).ToList();
            foreach (var toMoveSealedBox in style2SealedBoxes)
            {
                toMoveSealedBox.StyleId = styleTo.Id;
                log.Info("Moved sealed box=" + toMoveSealedBox.Id);
            }

            var style2OpenBoxes = db.OpenBoxes.GetAll().Where(b => b.StyleId == styleFrom.Id).ToList();
            foreach (var toMoveOpenBox in style2OpenBoxes)
            {
                toMoveOpenBox.StyleId = styleTo.Id;
                log.Info("Moved open box=" + toMoveOpenBox.Id);
            }
            db.Commit();

            
            //Update caches
            cacheService.RequestStyleIdUpdates(db, 
                new List<long>()
                {
                    styleTo.Id,
                    styleFrom.Id
                },
                UpdateCacheMode.IncludeChild, 
                by);

            //Delete style
            styleFrom.Deleted = true;
            styleFrom.UpdateDate = when;
            styleFrom.UpdatedBy = by;
            db.Commit();

            return true;
        }

        public override string ToString()
        {
            return "InputMainStyleId=" + InputMainStyleId 
                + ", InputSecondStyleId=" + InputSecondStyleId;
        }
    }
}