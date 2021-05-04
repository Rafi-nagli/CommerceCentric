using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Amazon.Core.Entities.Quickbooks
{
    public class ProductList
    {
        [Key]
        public int Id { get; set; }

        public string BNSKU { get; set; }
        
        public string MANFSKU { get; set; }

        public string QBSKU { get; set; }

        public string BRAND { get; set; }

        public string ProductDescription { get; set; }

        public string ProductTitle { get; set; }

        public string MSRP { get; set; }

        public string PRICE { get; set; }

        public string UPC { get; set; }

        public string Gender { get; set; }

        public string CaseShape { get; set; }

        public string WarrantyLength { get; set; }

        public string WarrantyType { get; set; }

        public string Hand { get; set; }

        public string Movement { get; set; }

        public string Material { get; set; }

        public string Type { get; set; }

        public string StrapColor { get; set; }

        public string DialColor { get; set; }

        public string Age { get; set; }

        public string WaterResistanceDepth { get; set; }

        public string CaseDiameter { get; set; }

        public string CaseThickness { get; set; }

        public string ClosureType { get; set; }

        public string Crown { get; set; }

        public string DialWindowMaterial { get; set; }

        public string Country { get; set; }

        public string ProductFeature1 { get; set; }

        public string ProductFeature2 { get; set; }
        
        public string ProductFeature3 { get; set; }

        public string StrapLenght { get; set; }

        public string StrapWidth { get; set; }

        public string CaseBackMaterial { get; set; }

        public string CaseBackType { get; set; }

        public string StoneAccent { get; set; }

        public string ExactColor { get; set; }
        
        public string Quality { get; set; }

        public string Bezel { get; set; }

        public string DialType { get; set; }
        
        public string Straptype { get; set; }

        public string StrapMaterial { get; set; }
        
        public string Calendar { get; set; }
        
        public string Crystal { get; set; }
        
        public string UPC_2 { get; set; }
        
        public int? Complete { get; set; }
        
        public string Qty { get; set; }

        public string Luminosity { get; set; }
    }
}
