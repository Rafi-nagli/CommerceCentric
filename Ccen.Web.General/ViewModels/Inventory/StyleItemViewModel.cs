﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Amazon.Core;
using Amazon.DTO;
using Amazon.DTO.Inventory;

namespace Amazon.Web.ViewModels.Inventory
{
    public class StyleItemViewModel
    {
        public long Id { get; set; }

        public string Size { get; set; }

        public string Color { get; set; }
        
        public double? Weight { get; set; }


        public bool AutoGeneratedBarcode { get; set; }

        public int? SizeId { get; set; }
        public int? SizeGroupId { get; set; }
        public string SizeGroupName { get; set; }

        public IList<BarcodeDTO> Barcodes { get; set; }


        //Additional for Box
        public int? Quantity { get; set; }
        public int? Breakdown { get; set; }
        public long? BoxItemId { get; set; }

        public decimal? Price { get; set; }

        public decimal? PackageLength { get; set; }
        public decimal? PackageWidth { get; set; }
        public decimal? PackageHeight { get; set; }



        public StyleItemViewModel()
        {

        }

        public StyleItemViewModel(StyleItemDTO styleItem)
        {
            Id = styleItem.StyleItemId;
            Size = styleItem.Size;
            Color = styleItem.Color;
            Weight = styleItem.Weight;


            Barcodes = styleItem.Barcodes;

            SizeId = styleItem.SizeId;
            SizeGroupId = styleItem.SizeGroupId;
            SizeGroupName = styleItem.SizeGroupName;

            PackageHeight = styleItem.PackageHeight;
            PackageWidth = styleItem.PackageWidth;
            PackageLength = styleItem.PackageLength;
        }

        public static bool HasLinkedEntities(IUnitOfWork db, long styleItemId)
        {
            var linkedListings = db.Items.GetAllViewAsDto().Count(l => l.StyleItemId == styleItemId);
            var openBoxItems = db.OpenBoxItems.GetAll().Count(op => op.StyleItemId == styleItemId);
            var sealedBoxItems = db.SealedBoxItems.GetAll().Count(op => op.StyleItemId == styleItemId);
            var specialCaseItems = db.QuantityChanges.GetAll().Count(op => op.StyleItemId == styleItemId);

            return linkedListings > 0
                   || openBoxItems > 0
                   || sealedBoxItems > 0
                   || specialCaseItems > 0;
        }
    }
}