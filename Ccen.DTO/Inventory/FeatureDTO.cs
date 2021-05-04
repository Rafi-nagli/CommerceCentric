using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amazon.DTO.Inventory
{
    public class FeatureDTO
    {
        public int Id { get; set; }
        public int? ItemTypeId { get; set; }
        public string Name { get; set; }
        public string EnName { get; set; }
        public string ZhName { get; set; }
        public string KrName { get; set; }
        public string TwName { get; set; }

        public string Notes { get; set; }
        public string ExtendedValue { get; set; }

        public int ValuesType { get; set; }
        public int Order { get; set; }

        public IList<FeatureValueDTO> FeatureValues { get; set; }
        public string Title { get; set; }
    }
}
