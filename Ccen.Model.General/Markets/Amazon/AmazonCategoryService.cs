using Amazon.Core.Contracts;
using Amazon.Core.Contracts.Factories;
using Amazon.Core.Models;
using Amazon.Core.Models.Categories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Model.General.Markets.Amazon
{
    public class AmazonCategoryService : IMarketCategoryService
    {
        private ILogService _log;
        private IDbFactory _dbFactory;
        private ITime _time;

        public AmazonCategoryService(ILogService log,
            ITime time,
            IDbFactory dbFactory)
        {
            _time = time;
            _dbFactory = dbFactory;
            _log = log;
        }

        public CategoryInfo GetCategory(MarketType market, 
            string marketplaceId, 
            string itemStyle, 
            string gender,
            SizeTypes sizeType)
        {
            /*										<xsd:enumeration value="Shirt"/>
										<xsd:enumeration value="Sweater"/>
										<xsd:enumeration value="Pants"/>
										<xsd:enumeration value="Shorts"/>
										<xsd:enumeration value="Skirt"/>
										<xsd:enumeration value="Dress"/>
										<xsd:enumeration value="Suit"/>
										<xsd:enumeration value="Blazer"/>
										<xsd:enumeration value="Outerwear"/>
										<xsd:enumeration value="SocksHosiery"/>
										<xsd:enumeration value="Underwear"/>
										<xsd:enumeration value="Bra"/>
										<xsd:enumeration value="Shoes"/>
										<xsd:enumeration value="Hat"/>
										<xsd:enumeration value="Bag"/>
										<xsd:enumeration value="Accessory"/>
										<xsd:enumeration value="Jewelry"/>
										<xsd:enumeration value="Sleepwear"/>
										<xsd:enumeration value="Swimwear"/>
										<xsd:enumeration value="PersonalBodyCare"/>
										<xsd:enumeration value="HomeAccessory"/>
										<xsd:enumeration value="NonApparelMisc"/>
										<xsd:enumeration value="Kimono"/>
										<xsd:enumeration value="Obi"/>
										<xsd:enumeration value="Chanchanko"/>
										<xsd:enumeration value="Jinbei"/>
										<xsd:enumeration value="Yukata"/>
										<xsd:enumeration value="EthnicWear"/>										
										<xsd:enumeration value="Costume"/>										
										<xsd:enumeration value="AdultCostume"/>										
										<xsd:enumeration value="BabyCostume"/>										
										<xsd:enumeration value="ChildrensCostume"/>	*/

            using (var db = _dbFactory.GetRWDb())
            {
                var category = db.AmazonCategoryMappings.GetAll()
                    .FirstOrDefault(c => c.Market == (int)market
                        && c.MarketplaceId == marketplaceId
                        && c.ItemStyle == itemStyle
                        && (c.Gender.Contains(gender) || String.IsNullOrEmpty(gender))
                        && (c.SizeType == (int)sizeType || c.SizeType == null));

                if (category != null)
                    return new CategoryInfo()
                    {
                        Key1 = category.ItemTypeKeyword,
                        Key2 = category.DepartmentKey,
                        Key3 = category.FeedType,
                        NodeIds = category.BrowseNodeIds,
                    };
                else
                    return new CategoryInfo()
                    {
                        Key1 = "",
                        Key2 = "",
                        Key3 = "",
                    };

                throw new NotSupportedException("Not found category for: ItemStyle=" + itemStyle + ", gender=" + gender + ", sizeType=" + sizeType);
            }
        }
    }
}
