using Amazon.Core.Entities.Enums;
using Amazon.Core.Models.DropShippers;
using Amazon.DTO.DropShippers;

namespace Amazon.Core.Models
{
    public class DSHelper
    {
        //GENERAL (for all portals)
        public static int DefaultDSId = 1;

        //PA
        public static int DefaultPAId = 1;
        public static int MBGPAId = 2;

        //ShipJoy
        public static int DefaultSJId = 1;

        //MBB
        public static int DefaultMBGId = 1;
        public static int PreorderId = 2;
        public static int OverseasId = 3;
        public static int PAonMBGId = 4;

        //Tonarex
        public static int DefaultTonarexId = 1;


        public static string GetMBGDsNameById(int? id)
        {
            if (id == DefaultMBGId)
                return "MBG";
            if (id == PreorderId)
                return "PreOrder";
            if (id == OverseasId)
                return "Overseas";
            if (id == PAonMBGId)
                return "PA";
            return "n/a";
        }

        //DWS
        public static int ZhuEntDsId = 3;
        public static int BNWDsId = 2;
        public static int GSDDsId = 1;
        public static int ZagerDsId = 4;
        public static int TwiDsId = 5;
        public static int StuhrlingDsId = 6;
        public static int AccuratimeDsId = 7;
        public static int CrotonJewleryDsId = 8;
        public static int StuhrlingOutletDsId = 10;
        public static int DWSDsId = 11;
        public static int SolarTimeDsId = 12;
        public static int VLCDsId = 13;
        public static int MomoDsId = 14;
        public static int NasivmovWatchId = 15;
        public static int UltraluxId = 16;
        public static int AshfordDsId = 17;
        public static int SignedPiecesId = 18;
        public static int CertifiedWatchStoreDsId = 19;

        public static int DesignerEyesDsId = 20;
        public static int PhilipSteinDsId = 21;


        public static string ZagerDsName = "Zager";
        public static string ZhuEntDsName = "ZhuEnt";
        public static string VLCDsName = "VLC";

        public static decimal? GetCost(DSItemDTO dsItem, decimal? salePrice)
        {
            if (dsItem != null)
            {
                if (dsItem.CostMode == (int) CostModes.PercentFromSalePrice)
                {
                    if (salePrice.HasValue)
                        return dsItem.CostPercent*salePrice;
                }
                if (dsItem.CostMode == (int) CostModes.FromFeed)
                    return dsItem.Cost;
            }
            //else
            //{
            //    //NOTE: temp for Zai items
            //    if (salePrice.HasValue)
            //        return 0.84M*salePrice.Value;
            //}
            return null;
        }

        public static int GetItemTypeIdFromDSProductType(int? productType)
        {
            var itemId = ItemType.Watches;
            if (productType.HasValue)
            {
                switch ((DSProductType)productType.Value)
                {
                    case DSProductType.Watches:
                        itemId = ItemType.Watches;
                        break;
                    case DSProductType.Sunglasses:
                        itemId = ItemType.Sunglasses;
                        break;
                    case DSProductType.Jewelry:
                        itemId = ItemType.Jawelry;
                        break;
                }
            }
            return itemId;
        }
    }
}
