using System;

namespace Amazon.DTO.Inventory
{
    public class StyleChangeHistoryDTO
    {
        public long Id { get; set; }

        public long StyleId { get; set; }
        public string FieldName { get; set; }
        public string FromValue { get; set; }
        public string ExtendFromValue { get; set; }
        public string ToValue { get; set; }
        public string ExtendToValue { get; set; }

        public long? ChangedBy { get; set; }
        public string ChangedByName { get; set; }
        public DateTime ChangeDate { get; set; }
    }
}
