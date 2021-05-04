using System.ComponentModel.DataAnnotations;
using Amazon.Core.Entities.Enums;

namespace Amazon.Core.Entities.Features
{
    public class Feature
    {
        [Key]
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
        public bool IsActive { get; set; }
        public string Title { get; set; }

    }
}
